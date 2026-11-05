using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.Models;

/// <summary>Profil for en ressursleverandør. Primærnøkkelen er også fremmednøkkel til <see cref="ApplicationUser"/> (class table inheritance).</summary>
public class ResourceProviderProfile
{
    [Key] public string UserId { get; set; } = string.Empty;   // PK + FK til ApplicationUser
    public ApplicationUser User { get; set; } = null!;

    public ProviderType ProviderType { get; set; }
    [MaxLength(30)] public string? ContactPhone { get; set; }

    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
