namespace FeedbackMap.Models;

/// <summary>Navn på rollene i systemet.</summary>
public static class Roles
{
    public const string PublicActor = "PublicActor";
    public const string ResourceProvider = "ResourceProvider";
}

/// <summary>Statusflyt fra user story #8.</summary>
public enum NeedStatus { New, UnderReview, Assigned, Resolved }

/// <summary>Kartfarger: rød = akutt, gul = planlagt.</summary>
public enum NeedPriority { Acute, Planned }

/// <summary>Ressursens tilgjengelighet slik leverandøren har satt den.</summary>
public enum ResourceStatus { Available, Unavailable }

/// <summary>Hvor langt en tildeling har kommet.</summary>
public enum MatchStatus { Proposed, Confirmed, Completed, Cancelled }

/// <summary>Type ressursleverandør (fra caset).</summary>
public enum ProviderType
{
    PrivatePerson, Business, Farmer, Contractor, VoluntaryOrganisation, DroneOperator
}
