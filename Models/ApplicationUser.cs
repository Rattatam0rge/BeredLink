using Microsoft.AspNetCore.Identity;

namespace FeedbackMap.Models;

/// <summary>ACTOR i datamodellen: delt innlogging (Identity).
/// Subtypene er egne profiltabeller (class table inheritance).</summary>
public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PublicActorProfile? PublicActorProfile { get; set; }
    public ResourceProviderProfile? ResourceProviderProfile { get; set; }
}
