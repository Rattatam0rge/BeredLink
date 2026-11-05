using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Leverer ressursbilder. Bare innloggede får se dem. Leverandører ser kun egne bilder.</summary>
[Authorize]
public class MediaController : Controller
{
    private readonly IResourceService _resources;
    private readonly IImageStorage _images;

    public MediaController(IResourceService resources, IImageStorage images)
    {
        _resources = resources;
        _images = images;
    }

    /// <summary>Sender bildet av en ressurs. Krever innlogging, og leverandører får bare se egne bilder.</summary>
    [HttpGet]
    public async Task<IActionResult> ResourceImage(int id)
    {
        var info = await _resources.GetImageAsync(id);
        if (info is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.IsInRole(Roles.ResourceProvider) && info.OwnerId != userId) return Forbid();

        var stream = _images.Open(info.FileName);
        if (stream is null) return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "private, max-age=3600";
        return File(stream, _images.ContentTypeFor(info.FileName));
    }
}
