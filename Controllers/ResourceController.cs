using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using FeedbackMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Ressurser: oversikt, registrering (med bilde), endring, sletting og utskrift av regionliste.</summary>
[Authorize]
public class ResourceController : Controller
{
    private readonly IResourceService _resources;
    private readonly IImageStorage _images;

    public ResourceController(IResourceService resources, IImageStorage images)
    {
        _resources = resources;
        _images = images;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Leverandør ser egne ressurser, offentlig aktør ser tilgjengelige ressurser
    /// <summary>Leverandør ser egne ressurser, offentlig aktør ser tilgjengelige ressurser.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var isProvider = User.IsInRole(Roles.ResourceProvider);
        var items = isProvider
            ? await _resources.ListForProviderAsync(UserId)
            : await _resources.ListAvailableAsync();

        return View(new ResourceIndexViewModel { IsProvider = isProvider, Items = items });
    }

    // Utskriftsvennlig ressursliste for en region (kartutsnitt) – case: «skrive ut ressurslister for sin region»
    /// <summary>Utskriftsvennlig ressursliste for en region (kartutsnitt).</summary>
    [HttpGet, Authorize(Roles = Roles.PublicActor)]
    public async Task<IActionResult> Print(decimal? minLat, decimal? maxLat, decimal? minLng, decimal? maxLng, string? type)
    {
        var hasBounds = minLat.HasValue && maxLat.HasValue && minLng.HasValue && maxLng.HasValue;
        var items = hasBounds
            ? await _resources.ListInBoundsAsync(minLat!.Value, maxLat!.Value, minLng!.Value, maxLng!.Value, type)
            : (await _resources.ListAvailableAsync()).Where(r => string.IsNullOrWhiteSpace(type) || r.Type == type).ToList();

        return View(new ResourcePrintViewModel
        {
            Items = items,
            Type = type,
            HasBounds = hasBounds,
            MinLat = minLat ?? 0, MaxLat = maxLat ?? 0, MinLng = minLng ?? 0, MaxLng = maxLng ?? 0
        });
    }

    // Registrering skjer i panelet på kartsiden
    /// <summary>Registrering skjer i panelet på kartsiden, så vi sender brukeren dit.</summary>
    [HttpGet, Authorize(Roles = Roles.ResourceProvider)]
    public IActionResult Create() => Redirect("/?register=1");

    /// <summary>Kalles fra registrer-panelet på kartet (fetch, multipart). Svarer med JSON.</summary>
    [HttpPost, Authorize(Roles = Roles.ResourceProvider), RequestSizeLimit(8_000_000)]
    public async Task<IActionResult> Create(ResourceCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        string? imageName = null;
        if (model.Image is { Length: > 0 })
        {
            var saved = await _images.SaveAsync(model.Image);
            if (!saved.Success)
            {
                var errors = new Dictionary<string, string[]> { ["Image"] = new[] { saved.Error! } };
                return BadRequest(new { success = false, errors });
            }
            imageName = saved.FileName;
        }

        int id;
        try
        {
            id = await _resources.CreateAsync(UserId, new ResourceInput(
                model.Type, model.Description, model.Latitude!.Value, model.Longitude!.Value,
                model.AvailableFrom!.Value, model.AvailableTo, imageName));
        }
        catch
        {
            if (imageName is not null) _images.Delete(imageName);   // ikke la bildet bli liggende uten ressurs
            throw;
        }

        return Ok(new
        {
            success = true,
            id,
            message = "Ressursen er registrert.",
            url = Url.Action(nameof(Index)),
            linkText = "Se mine ressurser",
            lat = model.Latitude,
            lng = model.Longitude
        });
    }

    /// <summary>Henter en egen ressurs som JSON, slik at endre-panelet på kartet kan fylles ut.</summary>
    [HttpGet, Authorize(Roles = Roles.ResourceProvider)]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var data = await _resources.GetForEditAsync(UserId, id);
        if (data is null) return NotFound();

        return Json(new
        {
            id = data.Id,
            type = data.Type,
            description = data.Description,
            latitude = data.Latitude,
            longitude = data.Longitude,
            availableFrom = data.AvailableFrom.ToString("yyyy-MM-ddTHH:mm"),
            availableTo = data.AvailableTo?.ToString("yyyy-MM-ddTHH:mm"),
            imageUrl = data.HasImage ? Url.Action("ResourceImage", "Media", new { id = data.Id }) : null
        });
    }

    /// <summary>Lagrer endringer i en egen ressurs (fetch, multipart). Svarer med JSON.</summary>
    [HttpPost, Authorize(Roles = Roles.ResourceProvider), RequestSizeLimit(8_000_000)]
    public async Task<IActionResult> Update(int id, ResourceCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        string? newImage = null;
        if (model.Image is { Length: > 0 })
        {
            var saved = await _images.SaveAsync(model.Image);
            if (!saved.Success)
            {
                var errors = new Dictionary<string, string[]> { ["Image"] = new[] { saved.Error! } };
                return BadRequest(new { success = false, errors });
            }
            newImage = saved.FileName;
        }

        var result = await _resources.UpdateAsync(UserId, id, new ResourceUpdateInput(
            model.Type, model.Description, model.Latitude!.Value, model.Longitude!.Value,
            model.AvailableFrom!.Value, model.AvailableTo, newImage, model.RemoveImage));

        if (!result.Success)
        {
            if (newImage is not null) _images.Delete(newImage);   // ikke la det nye bildet bli liggende
            if (result.Forbidden) return Forbid();
            var errors = new Dictionary<string, string[]> { [""] = new[] { result.Message } };
            return BadRequest(new { success = false, errors });
        }

        return Ok(new
        {
            success = true,
            message = result.Message,
            url = Url.Action(nameof(Index)),
            linkText = "Se mine ressurser",
            lat = model.Latitude,
            lng = model.Longitude
        });
    }

    /// <summary>Fjerner leverandørens egen ressurs (sletter eller setter inaktiv hvis den har historikk).</summary>
    [HttpPost, Authorize(Roles = Roles.ResourceProvider)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _resources.RemoveAsync(UserId, id);
        if (result.Forbidden) return Forbid();

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
