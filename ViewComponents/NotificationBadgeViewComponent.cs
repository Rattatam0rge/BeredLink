using System.Security.Claims;
using FeedbackMap.Models;
using FeedbackMap.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackMap.ViewComponents;

/// <summary>Varselklokke med antall uleste varsler i navbaren (kun for ressursleverandører).</summary>
public class NotificationBadgeViewComponent : ViewComponent
{
    private readonly INotificationService _notifications;

    public NotificationBadgeViewComponent(INotificationService notifications) => _notifications = notifications;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!UserClaimsPrincipal.IsInRole(Roles.ResourceProvider)) return Content(string.Empty);

        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Content(string.Empty);

        return View(await _notifications.UnreadCountAsync(userId));
    }
}
