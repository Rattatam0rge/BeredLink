using FeedbackMap.Models;

namespace FeedbackMap.Services;

/// <summary>Data som trengs for å registrere en ny ressurs.</summary>
public record ResourceInput(string Type, string? Description, decimal Latitude, decimal Longitude,
    DateTime AvailableFrom, DateTime? AvailableTo, string? ImageFileName = null);

/// <summary>Data for å endre en eksisterende ressurs. Bildet byttes hvis NewImageFileName er satt, og fjernes hvis RemoveImage er true.</summary>
public record ResourceUpdateInput(string Type, string? Description, decimal Latitude, decimal Longitude,
    DateTime AvailableFrom, DateTime? AvailableTo, string? NewImageFileName, bool RemoveImage);

/// <summary>Feltene som fylles inn i skjemaet når leverandøren endrer en ressurs.</summary>
public record ResourceEditData(int Id, string Type, string? Description, decimal Latitude, decimal Longitude,
    DateTime AvailableFrom, DateTime? AvailableTo, bool HasImage);

/// <summary>Én rad i ressurslister og på kartet, inkludert leverandørens kontaktinformasjon.</summary>
public record ResourceListItem(int Id, string Type, string? Description, decimal Latitude, decimal Longitude,
    DateTime AvailableFrom, DateTime? AvailableTo, ResourceStatus Status, ProviderType ProviderType,
    string? Phone, string? Email, bool HasImage, bool IsAssigned);

/// <summary>Hvem som eier en ressurs og hvilken bildefil den har (brukes til tilgangskontroll av bilder).</summary>
public record ResourceImageInfo(string OwnerId, string FileName);

/// <summary>Forretningslogikk for ressurser (user story #5, #6, #12).</summary>
public interface IResourceService
{
    /// <summary>Registrerer ressursen som tilgjengelig for leverandøren. Returnerer id.</summary>
    Task<int> CreateAsync(string userId, ResourceInput input);

    /// <summary>
    /// Henter feltene til en egen ressurs for redigering. Null hvis ressursen ikke finnes eller tilhører en annen leverandør.
    /// </summary>
    Task<ResourceEditData?> GetForEditAsync(string userId, int id);

    /// <summary>
    /// Endrer en ressurs. Bare eieren kan endre den, og ikke mens den er tildelt et behov.
    /// Et erstattet eller fjernet bilde slettes fra disk.
    /// </summary>
    Task<ServiceResult> UpdateAsync(string userId, int id, ResourceUpdateInput input);

    /// <summary>Leverandørens egne aktive ressurser.</summary>
    Task<IReadOnlyList<ResourceListItem>> ListForProviderAsync(string userId);

    /// <summary>Alle ressurser som er aktive, tilgjengelige akkurat nå og ikke tildelt et behov.</summary>
    Task<IReadOnlyList<ResourceListItem>> ListAvailableAsync();

    /// <summary>Tilgjengelige ressurser innenfor et geografisk område (kartutsnitt), eventuelt av én type.</summary>
    Task<IReadOnlyList<ResourceListItem>> ListInBoundsAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng, string? type);

    /// <summary>Bildefil og eier for en ressurs, eller null hvis den ikke har bilde.</summary>
    Task<ResourceImageInfo?> GetImageAsync(int id);

    /// <summary>
    /// Fjerner en ressurs for eieren. Uten historikk slettes raden. Med historikk settes den inaktiv slik at tidligere tildelinger bevares.
    /// Nektes mens ressursen er tildelt et behov.
    /// </summary>
    Task<ServiceResult> RemoveAsync(string userId, int id);
}
