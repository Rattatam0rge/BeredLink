using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.Models;

/// <summary>Sporbarhet: hvem gjorde hva og når (audit-logg fra casets utvidelser).</summary>
public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(450)] public string? UserId { get; set; }
    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string EntityType { get; set; } = string.Empty;
    [MaxLength(50)] public string? EntityId { get; set; }
    [MaxLength(500)] public string? Details { get; set; }
}
