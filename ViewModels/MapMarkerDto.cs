namespace FeedbackMap.ViewModels;

/// <summary>
/// Icon: "acute" | "planned" | "resource" | "actor". Kind: "need" | "resource" | "actor".
/// Suitable: for behov, hvilke ressurstyper som passer (brukes i radiusfilteret).
/// </summary>
public record MapMarkerDto(
    string Kind, string Icon, int Id, double Lat, double Lng,
    string Type, string Title, string Status, string When,
    string? Contact, string? Phone, string? Email, string? Url,
    string? ImageUrl = null, string[]? Suitable = null, bool CanEdit = false);
