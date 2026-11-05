using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.Controllers;

/// <summary>Varsler til ressursleverandører når en ressurs tildeles eller tildelingen avbrytes.</summary>
[Authorize(Roles = Roles.ResourceProvider)]
public class NotificationController : Controller
{
    private readonly INotificationService _notifications;

    public NotificationController(INotificationService notifications) => _notifications = notifications;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Leverandørens varsler.</summary>
    [HttpGet]
    public async Task<IActionResult> Index() => View(await _notifications.ListAsync(UserId));

    /// <summary>Markerer ett varsel som lest.</summary>
    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notifications.MarkReadAsync(UserId, id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Markerer alle varsler som lest.</summary>
    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifications.MarkAllReadAsync(UserId);
        return RedirectToAction(nameof(Index));
    }
}
