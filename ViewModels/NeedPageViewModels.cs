using FeedbackMap.Services;

namespace FeedbackMap.ViewModels;

/// <summary>Alt behovssiden trenger: behovet, tildelinger og ressurser i nærheten.</summary>
public class NeedDetailsPageViewModel
{
    public static readonly int[] Radii = { 500, 1000, 2000, 5000, 10000 };

    public NeedDetails Need { get; set; } = null!;
    public IReadOnlyList<AssignmentView> Assignments { get; set; } = Array.Empty<AssignmentView>();
    public IReadOnlyList<NearbyResource> Nearby { get; set; } = Array.Empty<NearbyResource>();
    public int Radius { get; set; } = 2000;
    public bool OnlySuitable { get; set; } = true;
}
