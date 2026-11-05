using Microsoft.AspNetCore.Http;

namespace FeedbackMap.Services;

/// <summary>Innstillinger for lagring av opplastede bilder (seksjonen «Storage» i konfigurasjonen).</summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Mappe for opplastede bilder. Tom = «uploads» ved siden av applikasjonen.</summary>
    public string? UploadsPath { get; set; }
}

/// <summary>Resultat av en bildeopplasting: filnavnet som ble lagret, eller en feilmelding til brukeren.</summary>
public record ImageSaveResult(bool Success, string? FileName, string? Error);

/// <summary>Lagring av bilder av ressurser på disk.</summary>
public interface IImageStorage
{
    /// <summary>Validerer og lagrer bildet. Filtypen avgjøres av innholdet, ikke av filnavnet.</summary>
    Task<ImageSaveResult> SaveAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>Åpner en lagret fil for lesing, eller null hvis den ikke finnes.</summary>
    Stream? Open(string fileName);

    /// <summary>Sletter en lagret fil (stille hvis den ikke finnes).</summary>
    void Delete(string fileName);

    /// <summary>Content-Type for en lagret fil, utledet fra filendelsen vi selv har satt.</summary>
    string ContentTypeFor(string fileName);
}
