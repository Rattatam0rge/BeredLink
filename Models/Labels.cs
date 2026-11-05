namespace FeedbackMap.Models;

/// <summary>Norske visningstekster for roller og enum-verdier (UI er norsk, koden engelsk).</summary>
public static class Labels
{
    public static string Role(string role) => role switch
    {
        Roles.PublicActor => "Offentlig aktør",
        Roles.ResourceProvider => "Ressursleverandør",
        _ => role
    };

    public static string Status(NeedStatus s) => s switch
    {
        NeedStatus.New => "Ny",
        NeedStatus.UnderReview => "Vurderes",
        NeedStatus.Assigned => "Tildelt",
        NeedStatus.Resolved => "Løst",
        _ => s.ToString()
    };

    public static string Priority(NeedPriority p) => p switch
    {
        NeedPriority.Acute => "Akutt",
        NeedPriority.Planned => "Planlagt",
        _ => p.ToString()
    };

    public static string Resource(ResourceStatus s) => s switch
    {
        ResourceStatus.Available => "Tilgjengelig",
        ResourceStatus.Unavailable => "Utilgjengelig",
        _ => s.ToString()
    };

    public static string Provider(ProviderType t) => t switch
    {
        ProviderType.PrivatePerson => "Privatperson",
        ProviderType.Business => "Bedrift",
        ProviderType.Farmer => "Bonde",
        ProviderType.Contractor => "Entreprenør",
        ProviderType.VoluntaryOrganisation => "Frivillig organisasjon",
        ProviderType.DroneOperator => "Droneoperatør",
        _ => t.ToString()
    };

    public static string Match(MatchStatus m) => m switch
    {
        MatchStatus.Proposed => "Foreslått",
        MatchStatus.Confirmed => "Bekreftet",
        MatchStatus.Completed => "Fullført",
        MatchStatus.Cancelled => "Avbrutt",
        _ => m.ToString()
    };

    public static string Distance(double meters) =>
        meters < 1000 ? $"{meters:F0} m" : $"{meters / 1000:F1} km";

    public static string Date(DateTime d) => d.ToString("dd.MM.yyyy HH:mm");

    public static string StatusBadge(NeedStatus s) => s switch
    {
        NeedStatus.New => "text-bg-primary",
        NeedStatus.UnderReview => "text-bg-info",
        NeedStatus.Assigned => "text-bg-dark",
        NeedStatus.Resolved => "text-bg-success",
        _ => "text-bg-secondary"
    };

    public static string PriorityBadge(NeedPriority p) =>
        p == NeedPriority.Acute ? "text-bg-danger" : "text-bg-warning";
}
