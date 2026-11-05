using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.Models;

/// <summary>En ressurs en leverandør kan tilby (utstyr, kjøretøy, mannskap, lokaler osv.).</summary>
public class Resource
{
    public int Id { get; set; }

    public string ResourceProviderId { get; set; } = string.Empty;
    public ResourceProviderProfile ResourceProvider { get; set; } = null!;

    [Required, MaxLength(100)] public string Type { get; set; } = string.Empty;   // Traktor, Drone, Aggregat ...
    [MaxLength(1000)] public string? Description { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Available;

    /// <summary>Filnavn på bildet av ressursen (lagres i uploads-volumet, vises via MediaController).</summary>
    [MaxLength(100)] public string? ImageFileName { get; set; }

    /// <summary>"Sletting" = IsActive=false hvis ressursen har assignments (user story #12).</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
