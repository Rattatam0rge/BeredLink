namespace FeedbackMap.Services;

/// <summary>Én hendelse i aktivitetsloggen, med brukerens e-post slått opp.</summary>
public record AuditItem(int Id, DateTime Timestamp, string? User, string Action, string EntityType,
    string? EntityId, string? Details);

/// <summary>Sporbarhet: hvem gjorde hva og når (audit-logg).</summary>
public interface IAuditService
{
    /// <summary>Legger til en loggrad i samme DbContext. Lagres sammen med neste SaveChanges.</summary>
    void Add(string? userId, string action, string entityType, string? entityId = null, string? details = null);

    /// <summary>Legger til og lagrer umiddelbart.</summary>
    Task LogAsync(string? userId, string action, string entityType, string? entityId = null, string? details = null);

    /// <summary>De siste hendelsene, nyeste først.</summary>
    Task<IReadOnlyList<AuditItem>> ListRecentAsync(int take = 200);
}
