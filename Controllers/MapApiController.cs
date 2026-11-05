using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using FeedbackMap.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>JSON-API som leverer markører til kartet, filtrert etter brukerens rolle.</summary>
[ApiController]
[Route("api/map")]
[Authorize]
public class MapApiController : ControllerBase
{
    private readonly INeedService _needs;
    private readonly IResourceService _resources;
    private readonly IActorService _actors;

    public MapApiController(INeedService needs, IResourceService resources, IActorService actors)
    {
        _needs = needs;
        _resources = resources;
        _actors = actors;
    }

    /// <summary>
    /// Offentlig aktør: åpne behov, tilgjengelige ressurser og andre offentlige aktørers posisjon.
    /// Ressursleverandør: kun egne ressurser (rollene har ulike rettigheter).
    /// </summary>
    [HttpGet("markers")]
    public async Task<ActionResult<IEnumerable<MapMarkerDto>>> Markers()
    {
        var markers = new List<MapMarkerDto>();

        if (User.IsInRole(Roles.PublicActor))
        {
            foreach (var n in await _needs.ListAsync(includeResolved: false))
            {
                markers.Add(new MapMarkerDto(
                    "need", n.Priority == NeedPriority.Acute ? "acute" : "planned", n.Id,
                    (double)n.Latitude, (double)n.Longitude, n.Type,
                    $"{Labels.Priority(n.Priority)} behov: {n.Type}",
                    Labels.Status(n.Status), $"Frist: {Labels.Date(n.Deadline)}",
                    n.ContactPoint, null, null,
                    Url.Action("Details", "Need", new { id = n.Id }),
                    null, Catalog.ResourceTypesFor(n.Type)));
            }

            foreach (var r in await _resources.ListAvailableAsync())
                markers.Add(ToMarker(r, canEdit: false));

            foreach (var a in await _actors.ListBasesAsync())
            {
                markers.Add(new MapMarkerDto(
                    "actor", "actor", 0, (double)a.Latitude, (double)a.Longitude, "Offentlig aktør",
                    a.Organisation,
                    a.Label ?? a.RoleTitle ?? "Offentlig aktør", string.Empty,
                    null, null, a.Email, null));
            }
        }
        else if (User.IsInRole(Roles.ResourceProvider))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            foreach (var r in await _resources.ListForProviderAsync(userId))
                markers.Add(ToMarker(r, canEdit: true));
        }

        return markers;
    }

    private MapMarkerDto ToMarker(ResourceListItem r, bool canEdit) => new(
        "resource", "resource", r.Id, (double)r.Latitude, (double)r.Longitude, r.Type,
        $"Ressurs: {r.Type}",
        r.IsAssigned ? "Tildelt et behov" : Labels.Resource(r.Status),
        r.AvailableTo is null
            ? $"Tilgjengelig fra {Labels.Date(r.AvailableFrom)}"
            : $"Tilgjengelig {Labels.Date(r.AvailableFrom)} – {Labels.Date(r.AvailableTo.Value)}",
        null, r.Phone, r.Email, null,
        r.HasImage ? Url.Action("ResourceImage", "Media", new { id = r.Id }) : null,
        null, canEdit);
}
