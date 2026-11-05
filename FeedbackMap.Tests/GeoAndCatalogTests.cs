using FeedbackMap.Models;
using FeedbackMap.Services;

namespace FeedbackMap.Tests;

public class GeoAndCatalogTests
{
    [Fact]
    public void Distance_SamePoint_IsZero() =>
        Assert.Equal(0, Geo.DistanceMeters(58.1467, 7.9956, 58.1467, 7.9956), 3);

    [Fact]
    public void Distance_OsloToBergen_IsAbout305Km()
    {
        var meters = Geo.DistanceMeters(59.9139, 10.7522, 60.3913, 5.3221);
        Assert.InRange(meters, 300_000, 310_000);
    }

    [Fact]
    public void Distance_500MetersNorth_IsAbout500()
    {
        // 0,0045 grader bredde er ca. 500 m
        var meters = Geo.DistanceMeters(58.0, 8.0, 58.0045, 8.0);
        Assert.InRange(meters, 495, 505);
    }

    [Theory]
    [InlineData("Strøm/aggregat", "Aggregat", true)]
    [InlineData("Strøm/aggregat", "Traktor", false)]
    [InlineData("Snørydding", "Traktor", true)]
    [InlineData("Droneobservasjon", "Drone og droneoperatør", true)]
    [InlineData("Samband", "Båt", false)]
    [InlineData("Annet", "Båt", true)]      // «Annet» passer til alt
    public void IsSuitable_MatchesCategories(string need, string resource, bool expected) =>
        Assert.Equal(expected, Catalog.IsSuitable(need, resource));

    [Fact]
    public void NorwegianErrorDescriber_UsesNorwegianText()
    {
        var describer = new NorwegianIdentityErrorDescriber();
        Assert.Contains("stor bokstav", describer.PasswordRequiresUpper().Description);
        Assert.Contains("liten bokstav", describer.PasswordRequiresLower().Description);
        Assert.Contains("10", describer.PasswordTooShort(10).Description);
    }
}
