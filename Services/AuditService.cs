using FeedbackMap.Data;
using FeedbackMap.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Implementasjon av <see cref="IAuditService"/>.</summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db) => _db = db;

    public void Add(string? userId, string action, string entityType, string? entityId = null, string? details = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details is { Length: > 500 } ? details[..500] : details
        });
    }

    public async Task LogAsync(string? userId, string action, string entityType, string? entityId = null, string? details = null)
    {
        Add(userId, action, entityType, entityId, details);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AuditItem>> ListRecentAsync(int take = 200)
    {
        var rows = await (
            from a in _db.AuditLogs.AsNoTracking()
            join u in _db.Users.AsNoTracking() on a.UserId equals u.Id into g
            from u in g.DefaultIfEmpty()
            orderby a.Timestamp descending, a.Id descending
            select new { a.Id, a.Timestamp, Email = u != null ? u.Email : null, a.Action, a.EntityType, a.EntityId, a.Details }
        ).Take(take).ToListAsync();

        return rows.Select(r => new AuditItem(r.Id, r.Timestamp, r.Email, r.Action, r.EntityType, r.EntityId, r.Details)).ToList();
    }
}
