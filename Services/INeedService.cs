using FeedbackMap.Models;

namespace FeedbackMap.Services;

/// <summary>Data som trengs for å registrere et nytt behov.</summary>
public record NeedInput(string Type, string? Description, decimal Latitude, decimal Longitude,
    DateTime Deadline, NeedPriority Priority, string ContactPoint);

/// <summary>Én rad i behovslisten og på kartet.</summary>
public record NeedListItem(int Id, string Type, NeedPriority Priority, NeedStatus Status, DateTime Deadline,
    string Organisation, decimal Latitude, decimal Longitude, string ContactPoint);

/// <summary>Én endring i statushistorikken til et behov.</summary>
public record StatusHistoryItem(NeedStatus Status, DateTime ChangedAt, string ChangedBy);

/// <summary>Alt som vises på detaljsiden til et behov, inkludert hvilke statuser det kan gå videre til.</summary>
public record NeedDetails(int Id, string Type, string? Description, NeedPriority Priority, NeedStatus Status,
    DateTime Deadline, string ContactPoint, decimal Latitude, decimal Longitude, string Organisation,
    DateTime CreatedAt, IReadOnlyList<StatusHistoryItem> History, IReadOnlyList<NeedStatus> NextStatuses);

/// <summary>Forretningslogikk for behov (user story #1, #8, #11).</summary>
public interface INeedService
{
    /// <summary>Oppretter behovet og logger statusen «Ny» automatisk. Returnerer id.</summary>
    Task<int> CreateAsync(string userId, NeedInput input);

    /// <summary>Alle behov, akutte først og deretter etter frist. Løste behov tas bare med hvis ønsket.</summary>
    Task<IReadOnlyList<NeedListItem>> ListAsync(bool includeResolved);

    /// <summary>Detaljer og statushistorikk, eller null hvis behovet ikke finnes.</summary>
    Task<NeedDetails?> GetDetailsAsync(int id);

    /// <summary>
    /// Endrer status etter flyten ny → vurderes → tildelt → løst (og tildelt → vurderes). Ugyldige overganger avvises.
    /// Når behovet settes til løst, fullføres aktive tildelinger og ressursene frigjøres.
    /// </summary>
    Task<ServiceResult> ChangeStatusAsync(int id, NeedStatus newStatus, string userId);
}
