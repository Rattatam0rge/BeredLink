using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using FeedbackMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Behov: liste, detaljer, registrering fra kartet, statusendring og tildeling av ressurser. Kun for offentlige aktører.</summary>
[Authorize(Roles = Roles.PublicActor)]
public class NeedController : Controller
{
    private readonly INeedService _needs;
    private readonly IAssignmentService _assignments;

    public NeedController(INeedService needs, IAssignmentService assignments)
    {
        _needs = needs;
        _assignments = assignments;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Liste over behov, akutte først.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(bool showResolved = false)
    {
        ViewData["ShowResolved"] = showResolved;
        return View(await _needs.ListAsync(showResolved));
    }

    // Registrering skjer i panelet på kartsiden
    /// <summary>Registrering skjer i panelet på kartsiden, så vi sender brukeren dit.</summary>
    [HttpGet]
    public IActionResult Create() => Redirect("/?register=1");

    /// <summary>Kalles fra registrer-panelet på kartet (fetch). Svarer med JSON.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(NeedCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        var id = await _needs.CreateAsync(UserId, new NeedInput(
            model.Type, model.Description, model.Latitude!.Value, model.Longitude!.Value,
            model.Deadline!.Value, model.Priority, model.ContactPoint));

        return Ok(new
        {
            success = true,
            id,
            message = "Behovet er registrert.",
            url = Url.Action(nameof(Details), new { id }),
            linkText = "Åpne behov",
            lat = model.Latitude,
            lng = model.Longitude
        });
    }

    /// <summary>Behovssiden: detaljer, tildelte ressurser og ressurser i nærheten innenfor valgt radius.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id, int radius = 2000, bool suitable = true)
    {
        var details = await _needs.GetDetailsAsync(id);
        if (details is null) return NotFound();

        if (!NeedDetailsPageViewModel.Radii.Contains(radius)) radius = 2000;

        var page = new NeedDetailsPageViewModel
        {
            Need = details,
            Radius = radius,
            OnlySuitable = suitable,
            Assignments = await _assignments.ListForNeedAsync(id),
            Nearby = details.Status == NeedStatus.Resolved
                ? Array.Empty<NearbyResource>()
                : await _assignments.FindNearbyAsync(id, radius, suitable)
        };
        return View(page);
    }

    /// <summary>Endrer status på behovet etter flyten ny → vurderes → tildelt → løst.</summary>
    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, NeedStatus status)
    {
        var result = await _needs.ChangeStatusAsync(id, status, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Tildeler en ressurs til behovet og varsler leverandøren.</summary>
    [HttpPost]
    public async Task<IActionResult> Assign(int id, int resourceId, int radius = 2000, bool suitable = true)
    {
        var result = await _assignments.AssignAsync(id, resourceId, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id, radius, suitable });
    }

    /// <summary>Avbryter en tildeling.</summary>
    [HttpPost]
    public async Task<IActionResult> Unassign(int id, int assignmentId, int radius = 2000, bool suitable = true)
    {
        var result = await _assignments.UnassignAsync(assignmentId, UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id, radius, suitable });
    }
}
