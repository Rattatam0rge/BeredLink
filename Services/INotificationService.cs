namespace FeedbackMap.Services;

/// <summary>Ett varsel til en ressursleverandør.</summary>
public record NotificationItem(int Id, string Message, DateTime CreatedAt, bool IsRead);

/// <summary>Varsler til leverandører når en ressurs tildeles eller tildelingen avbrytes (user story #9).</summary>
public interface INotificationService
{
    /// <summary>Leverandørens varsler, nyeste først.</summary>
    Task<IReadOnlyList<NotificationItem>> ListAsync(string providerId, int take = 100);

    /// <summary>Antall uleste varsler (vises i menyen).</summary>
    Task<int> UnreadCountAsync(string providerId);

    /// <summary>Markerer ett varsel som lest. Returnerer false hvis varselet ikke tilhører leverandøren.</summary>
    Task<bool> MarkReadAsync(string providerId, int notificationId);

    /// <summary>Markerer alle varsler som lest.</summary>
    Task MarkAllReadAsync(string providerId);
}
