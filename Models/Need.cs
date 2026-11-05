using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedbackMap.Models;

/// <summary>Et behov for bistand, registrert av en offentlig aktør.</summary>
public class Need
{
    public int Id { get; set; }

    public string PublicActorId { get; set; } = string.Empty;
    public PublicActorProfile PublicActor { get; set; } = null!;

    [Required, MaxLength(100)] public string Type { get; set; } = string.Empty;   // f.eks. Aggregat, Transport
    [MaxLength(1000)] public string? Description { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime Deadline { get; set; }
    public NeedPriority Priority { get; set; }
    [Required, MaxLength(200)] public string ContactPoint { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<NeedStatusLog> StatusLog { get; set; } = new List<NeedStatusLog>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    /// <summary>Utledet fra siste loggrad (krever .Include(n => n.StatusLog)).</summary>
    [NotMapped]
    public NeedStatus CurrentStatus =>
        StatusLog.OrderByDescending(s => s.ChangedAt).ThenByDescending(s => s.Id).Select(s => (NeedStatus?)s.Status).FirstOrDefault()
        ?? NeedStatus.New;
}
