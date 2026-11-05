namespace FeedbackMap.Models;

/// <summary>Tildeling av en ressurs til et behov. Koblingstabell mellom <see cref="Need"/> og <see cref="Resource"/>.</summary>
public class Assignment
{
    public int Id { get; set; }

    public int NeedId { get; set; }
    public Need Need { get; set; } = null!;

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
    public MatchStatus MatchStatus { get; set; } = MatchStatus.Proposed;

    /// <summary>
    /// Lik ResourceId så lenge tildelingen er aktiv, ellers null. Unik indeks på kolonnen gjør at en ressurs
    /// aldri kan ha to aktive tildelinger (MariaDB tillater flere NULL i en unik indeks).
    /// </summary>
    public int? ActiveResourceKey { get; set; }

    public Notification? Notification { get; set; }
}
