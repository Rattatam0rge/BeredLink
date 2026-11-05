namespace FeedbackMap.Models;

/// <summary>En endring i statusen til et behov. Behovets gjeldende status utledes fra siste rad.</summary>
public class NeedStatusLog
{
    public int Id { get; set; }

    public int NeedId { get; set; }
    public Need Need { get; set; } = null!;

    public NeedStatus Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string ChangedByActorId { get; set; } = string.Empty;
    public ApplicationUser ChangedByActor { get; set; } = null!;
}
