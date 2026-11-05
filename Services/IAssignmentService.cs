namespace FeedbackMap.Services;

/// <summary>En ressurs i nærheten av et behov, med avstand og om typen passer til behovet.</summary>
public record NearbyResource(ResourceListItem Resource, double DistanceMeters, bool TypeMatches);

/// <summary>En tildeling slik den vises på behovssiden.</summary>
public record AssignmentView(int Id, int ResourceId, string ResourceType, string? Phone, string? Email,
    DateTime AssignedAt, DateTime? UnassignedAt, FeedbackMap.Models.MatchStatus Status, bool IsActive);

/// <summary>Matching mellom behov og ressurser (user story #3, #8, #9).</summary>
public interface IAssignmentService
{
    /// <summary>Tilgjengelige ressurser innenfor radius (meter), sortert på avstand.</summary>
    Task<IReadOnlyList<NearbyResource>> FindNearbyAsync(int needId, double radiusMeters, bool onlySuitableTypes);

    /// <summary>
    /// Tildeler ressursen til behovet. Setter statusen til «Tildelt» (via «Vurderes») og varsler leverandøren.
    /// Avvises hvis ressursen allerede er tildelt, er utilgjengelig, eller behovet er løst.
    /// </summary>
    Task<ServiceResult> AssignAsync(int needId, int resourceId, string userId);

    /// <summary>Avbryter en aktiv tildeling. Ressursen blir ledig og leverandøren varsles.</summary>
    Task<ServiceResult> UnassignAsync(int assignmentId, string userId);

    /// <summary>Alle tildelinger for et behov (aktive og avsluttede), nyeste først.</summary>
    Task<IReadOnlyList<AssignmentView>> ListForNeedAsync(int needId);
}
