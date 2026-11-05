using System.ComponentModel.DataAnnotations;

namespace FeedbackMap.Models;

/// <summary>Profil for en offentlig aktør. Primærnøkkelen er også fremmednøkkel til <see cref="ApplicationUser"/> (class table inheritance).</summary>
public class PublicActorProfile
{
    [Key] public string UserId { get; set; } = string.Empty;   // PK + FK til ApplicationUser
    public ApplicationUser User { get; set; } = null!;

    [Required, MaxLength(150)] public string Organisation { get; set; } = string.Empty;
    [MaxLength(100)] public string? RoleTitle { get; set; }

    /// <summary>Organisasjonens posisjon (stasjon, base, kommunehus). Vises som blå markør på kartet.</summary>
    public decimal? BaseLatitude { get; set; }
    public decimal? BaseLongitude { get; set; }
    [MaxLength(100)] public string? BaseLabel { get; set; }

    public ICollection<Need> Needs { get; set; } = new List<Need>();
}
