namespace FeedbackMap.Services;

/// <summary>Resultat av en tjenestehandling: lyktes den, en melding til brukeren, og om feilen skyldes manglende tilgang.</summary>
public record ServiceResult(bool Success, string Message, bool Forbidden = false)
{
    public static ServiceResult Ok(string message) => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);
    public static ServiceResult Deny() => new(false, "Du har ikke tilgang til dette.", true);
}
