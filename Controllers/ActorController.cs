using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using FeedbackMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Lar offentlige aktører sette organisasjonens posisjon (blå markør på kartet).</summary>
[Authorize(Roles = Roles.PublicActor)]
public class ActorController : Controller
{
    private readonly IActorService _actors;

    public ActorController(IActorService actors) => _actors = actors;

    /// <summary>Kalles fra panelet på kartet (fetch). Setter organisasjonens posisjon (blå markør).</summary>
    [HttpPost]
    public async Task<IActionResult> SetBase(ActorBaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _actors.SetBaseAsync(userId, model.Latitude!.Value, model.Longitude!.Value, model.Label);
        if (result.Forbidden) return Forbid();

        return Ok(new { success = result.Success, message = result.Message, url = (string?)null, lat = model.Latitude, lng = model.Longitude });
    }
}
