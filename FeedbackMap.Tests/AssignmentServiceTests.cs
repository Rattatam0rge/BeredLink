using FeedbackMap.Models;
using FeedbackMap.Services;

namespace FeedbackMap.Tests;

public class AssignmentServiceTests
{
    [Fact]
    public async Task Assign_SetsNeedAssigned_CreatesAssignment_AndNotifiesProvider()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync();

        var result = await f.Assignments.AssignAsync(need, resource, Fixture.Actor);

        Assert.True(result.Success);
        var details = await f.Needs.GetDetailsAsync(need);
        Assert.Equal(NeedStatus.Assigned, details!.Status);
        // ny -> vurderes -> tildelt: flyten følges
        Assert.Contains(details.History, h => h.Status == NeedStatus.UnderReview);

        var assignment = Assert.Single(f.Db.Assignments);
        Assert.Equal(MatchStatus.Confirmed, assignment.MatchStatus);
        Assert.Equal(resource, assignment.ActiveResourceKey);

        var notifications = await f.Notifications.ListAsync(Fixture.Provider);
        var notification = Assert.Single(notifications);
        Assert.False(notification.IsRead);
        Assert.Contains("Strøm/aggregat", notification.Message);   // behovstype
        Assert.Contains("Kari", notification.Message);              // kontaktperson
        Assert.Equal(1, await f.Notifications.UnreadCountAsync(Fixture.Provider));
    }

    [Fact]
    public async Task Assign_SameResourceTwice_IsRejected_NoDoubleBooking()
    {
        using var f = new Fixture();
        var need1 = await f.NeedAsync();
        var need2 = await f.NeedAsync();
        var resource = await f.ResourceAsync();

        Assert.True((await f.Assignments.AssignAsync(need1, resource, Fixture.Actor)).Success);
        var second = await f.Assignments.AssignAsync(need2, resource, Fixture.Actor);

        Assert.False(second.Success);
        Assert.Single(f.Db.Assignments);
    }

    [Fact]
    public async Task Assign_ResolvedNeed_IsRejected()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        await f.Needs.ChangeStatusAsync(need, NeedStatus.UnderReview, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(need, NeedStatus.Assigned, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(need, NeedStatus.Resolved, Fixture.Actor);
        var resource = await f.ResourceAsync();

        Assert.False((await f.Assignments.AssignAsync(need, resource, Fixture.Actor)).Success);
    }

    [Fact]
    public async Task Assign_ExpiredResource_IsRejected()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var expired = await f.ResourceAsync(availableTo: DateTime.UtcNow.AddMinutes(-5));

        Assert.False((await f.Assignments.AssignAsync(need, expired, Fixture.Actor)).Success);
    }

    [Fact]
    public async Task Unassign_FreesResource_ReturnsNeedToUnderReview_AndNotifiesProvider()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);
        var assignmentId = f.Db.Assignments.Single().Id;

        var result = await f.Assignments.UnassignAsync(assignmentId, Fixture.Actor);

        Assert.True(result.Success);
        var assignment = f.Db.Assignments.Single();
        Assert.Equal(MatchStatus.Cancelled, assignment.MatchStatus);
        Assert.Null(assignment.ActiveResourceKey);
        Assert.NotNull(assignment.UnassignedAt);
        Assert.Equal(NeedStatus.UnderReview, (await f.Needs.GetDetailsAsync(need))!.Status);

        var notification = Assert.Single(await f.Notifications.ListAsync(Fixture.Provider));
        Assert.Contains("avbrutt", notification.Message);
        Assert.False(notification.IsRead);

        // ressursen kan tildeles på nytt
        var again = await f.Assignments.AssignAsync(need, resource, Fixture.Actor);
        Assert.True(again.Success);
    }

    [Fact]
    public async Task Unassign_Twice_IsRejected()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);
        var id = f.Db.Assignments.Single().Id;

        await f.Assignments.UnassignAsync(id, Fixture.Actor);

        Assert.False((await f.Assignments.UnassignAsync(id, Fixture.Actor)).Success);
    }

    [Fact]
    public async Task FindNearby_RespectsRadius_AndSortsByDistance()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var far = await f.ResourceAsync(lat: Fixture.Lat + 0.027m);      // ca. 3 km
        var near = await f.ResourceAsync(lat: Fixture.Lat + 0.0018m);    // ca. 200 m
        var mid = await f.ResourceAsync(lat: Fixture.Lat + 0.0072m);     // ca. 800 m

        var within1Km = await f.Assignments.FindNearbyAsync(need, 1000, onlySuitableTypes: false);
        var within5Km = await f.Assignments.FindNearbyAsync(need, 5000, onlySuitableTypes: false);

        Assert.Equal(new[] { near, mid }, within1Km.Select(r => r.Resource.Id).ToArray());
        Assert.InRange(within1Km[0].DistanceMeters, 190, 210);
        Assert.Equal(new[] { near, mid, far }, within5Km.Select(r => r.Resource.Id).ToArray());
    }

    [Fact]
    public async Task FindNearby_OnlySuitable_FiltersByCategory()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync("Strøm/aggregat");
        var generator = await f.ResourceAsync("Aggregat");
        await f.ResourceAsync("Traktor");

        var suitable = await f.Assignments.FindNearbyAsync(need, 1000, onlySuitableTypes: true);
        var everything = await f.Assignments.FindNearbyAsync(need, 1000, onlySuitableTypes: false);

        Assert.Equal(generator, Assert.Single(suitable).Resource.Id);
        Assert.Equal(2, everything.Count);
    }

    [Fact]
    public async Task FindNearby_ExcludesAlreadyAssignedResources()
    {
        using var f = new Fixture();
        var need1 = await f.NeedAsync();
        var need2 = await f.NeedAsync();
        var taken = await f.ResourceAsync();
        await f.ResourceAsync();
        await f.Assignments.AssignAsync(need1, taken, Fixture.Actor);

        var nearby = await f.Assignments.FindNearbyAsync(need2, 1000, onlySuitableTypes: false);

        Assert.Single(nearby);
        Assert.DoesNotContain(nearby, n => n.Resource.Id == taken);
    }
}
