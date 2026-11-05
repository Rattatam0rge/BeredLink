namespace FeedbackMap.Services;

/// <summary>En offentlig aktørs posisjon (stasjon, base, kommunehus) som vises som blå markør.</summary>
public record ActorBase(string UserId, string Organisation, string? RoleTitle, string? Label,
    decimal Latitude, decimal Longitude, string? Email);

/// <summary>Forretningslogikk for offentlige aktørers posisjon på kartet.</summary>
public interface IActorService
{
    /// <summary>Lagrer (eller flytter) organisasjonens posisjon. Bare offentlige aktører har profil.</summary>
    Task<ServiceResult> SetBaseAsync(string userId, decimal latitude, decimal longitude, string? label);

    /// <summary>Alle offentlige aktører som har satt posisjon.</summary>
    Task<IReadOnlyList<ActorBase>> ListBasesAsync();
}
