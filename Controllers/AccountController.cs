using FeedbackMap.Data;
using FeedbackMap.Models;
using FeedbackMap.Services;
using FeedbackMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Controllers;

/// <summary>Registrering, innlogging og «Min side».</summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AccountController> _logger;
    private readonly IAuditService _audit;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        ApplicationDbContext db,
        ILogger<AccountController> logger,
        IAuditService audit)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
        _logger = logger;
        _audit = audit;
    }

    // ---------- Registrering ----------

    /// <summary>Viser registreringsskjemaet.</summary>
    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    /// <summary>Oppretter bruker med rolle og profil (lagres i databasen), og logger inn.</summary>
    [HttpPost, AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var msg in result.Errors.Select(e => e.Description).Distinct()) ModelState.AddModelError(string.Empty, msg);
            return View(model);
        }

        try
        {
            await _users.AddToRoleAsync(user, model.Role);

            if (model.Role == Roles.PublicActor)
            {
                _db.PublicActorProfiles.Add(new PublicActorProfile
                {
                    UserId = user.Id,
                    Organisation = model.Organisation!.Trim(),
                    RoleTitle = model.RoleTitle?.Trim()
                });
            }
            else
            {
                _db.ResourceProviderProfiles.Add(new ResourceProviderProfile
                {
                    UserId = user.Id,
                    ProviderType = model.ProviderType!.Value,
                    ContactPhone = model.ContactPhone?.Trim()
                });
            }
            await _db.SaveChangesAsync();
        }
        catch
        {
            await _users.DeleteAsync(user);   // ingen halvferdige brukere
            throw;
        }

        _logger.LogInformation("Ny bruker registrert med rolle {Role}", model.Role);
        await _audit.LogAsync(user.Id, "UserRegistered", "User", user.Id, model.Role);

        await _signIn.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    // ---------- Innlogging ----------

    /// <summary>Viser innloggingsskjemaet.</summary>
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    /// <summary>
    /// Logger inn med e-post og passord. Kontoen låses i 5 minutter etter 3 feil passord.
    /// Samme feilmelding vises uansett om e-posten finnes, slik at man ikke kan lete etter brukere.
    /// </summary>
    [HttpPost, AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            var check = await _signIn.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (check.IsLockedOut)
            {
                await _audit.LogAsync(user.Id, "AccountLockedOut", "User", user.Id);
                return View("Lockout");
            }
            if (check.Succeeded)
            {
                await _signIn.SignInAsync(user, model.RememberMe);
                await _audit.LogAsync(user.Id, "LoginSucceeded", "User", user.Id);
                return RedirectToLocal(model.ReturnUrl);
            }
        }

        await _audit.LogAsync(user?.Id, "LoginFailed", "User", user?.Id);
        ModelState.AddModelError(string.Empty, "Feil e-post eller passord.");
        return View(model);
    }

    /// <summary>Logger ut.</summary>
    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Vises når brukeren mangler tilgang.</summary>
    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ---------- Min side ----------

    /// <summary>«Min side» med e-post, rolle og organisasjon.</summary>
    [HttpGet, Authorize]
    public async Task<IActionResult> Manage()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var profile = await _db.PublicActorProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        return View(new ManageViewModel
        {
            Email = user.Email ?? string.Empty,
            Roles = await _users.GetRolesAsync(user),
            Organisation = profile?.Organisation
        });
    }

    // ---------- Hjelpere ----------

    /// <summary>Sender brukeren til returnUrl hvis den er lokal (hindrer åpen omdirigering), ellers til forsiden.</summary>
    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
