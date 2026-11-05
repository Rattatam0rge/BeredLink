using FeedbackMap.Data;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Implementasjon av <see cref="INotificationService"/>. Et varsel tilhører leverandøren som eier den tildelte ressursen.</summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationItem>> ListAsync(string providerId, int take = 100)
    {
        var rows = await _db.Notifications.AsNoTracking()
            .Where(n => n.Assignment.Resource.ResourceProviderId == providerId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new { n.Id, n.Message, n.CreatedAt, n.IsRead })
            .ToListAsync();

        return rows.Select(r => new NotificationItem(r.Id, r.Message, r.CreatedAt, r.IsRead)).ToList();
    }

    public Task<int> UnreadCountAsync(string providerId) =>
        _db.Notifications.CountAsync(n => !n.IsRead && n.Assignment.Resource.ResourceProviderId == providerId);

    public async Task<bool> MarkReadAsync(string providerId, int notificationId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x =>
            x.Id == notificationId && x.Assignment.Resource.ResourceProviderId == providerId);
        if (n is null) return false;

        n.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync(string providerId)
    {
        var unread = await _db.Notifications
            .Where(n => !n.IsRead && n.Assignment.Resource.ResourceProviderId == providerId)
            .ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
