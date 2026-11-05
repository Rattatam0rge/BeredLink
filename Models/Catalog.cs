namespace FeedbackMap.Models;

/// <summary>Typer fra caset. Utvides senere til egne kategorier (case: "Kategorier for ressurstyper").</summary>
public static class Catalog
{
    public static readonly string[] NeedTypes =
    {
        "Transport", "Droneobservasjon", "Strøm/aggregat", "Snørydding", "Sand/grus",
        "Maskiner", "Evakuering", "Samband", "Mannskap", "Lokaler", "Annet"
    };

    public static readonly string[] ResourceTypes =
    {
        "Drone og droneoperatør", "ATV", "Traktor", "Gravemaskin", "Hjullaster", "Skogsmaskin",
        "Snøskuter", "Lastebil", "Sand/grus", "Aggregat", "Lokaler", "Sambandsutstyr", "Båt",
        "Mannskap/frivillige", "Annet"
    };

    /// <summary>Hvilke ressurstyper som passer til en behovstype (brukes i matchingen).</summary>
    private static readonly Dictionary<string, string[]> Suitable = new()
    {
        ["Transport"] = new[] { "Lastebil", "ATV", "Snøskuter" },
        ["Droneobservasjon"] = new[] { "Drone og droneoperatør" },
        ["Strøm/aggregat"] = new[] { "Aggregat" },
        ["Snørydding"] = new[] { "Traktor", "Hjullaster", "Snøskuter", "Lastebil", "Gravemaskin" },
        ["Sand/grus"] = new[] { "Sand/grus", "Lastebil", "Hjullaster" },
        ["Maskiner"] = new[] { "Gravemaskin", "Hjullaster", "Skogsmaskin", "Traktor" },
        ["Evakuering"] = new[] { "Lastebil", "Båt", "ATV", "Snøskuter", "Mannskap/frivillige" },
        ["Samband"] = new[] { "Sambandsutstyr" },
        ["Mannskap"] = new[] { "Mannskap/frivillige" },
        ["Lokaler"] = new[] { "Lokaler" }
    };

    public static string[] ResourceTypesFor(string needType) =>
        Suitable.TryGetValue(needType, out var types) ? types : ResourceTypes;

    public static bool IsSuitable(string needType, string resourceType) =>
        ResourceTypesFor(needType).Contains(resourceType);
}
