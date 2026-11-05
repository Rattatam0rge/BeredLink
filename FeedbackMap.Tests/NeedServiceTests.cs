using FeedbackMap.Models;
using FeedbackMap.Services;

namespace FeedbackMap.Tests;

public class NeedServiceTests
{
    [Fact]
    public async Task CreateNeed_StartsWithStatusNew_AndLogsIt()
    {
        using var f = new Fixture();
        var id = await f.NeedAsync();

        var details = await f.Needs.GetDetailsAsync(id);

        Assert.NotNull(details);
        Assert.Equal(NeedStatus.New, details!.Status);
        Assert.Single(details.History);
        Assert.Equal("Test kommune", details.Organisation);
    }

    [Fact]
    public async Task ChangeStatus_FollowsFlow_NewToResolved()
    {
        using var f = new Fixture();
        var id = await f.NeedAsync();

        Assert.True((await f.Needs.ChangeStatusAsync(id, NeedStatus.UnderReview, Fixture.Actor)).Success);
        Assert.True((await f.Needs.ChangeStatusAsync(id, NeedStatus.Assigned, Fixture.Actor)).Success);
        Assert.True((await f.Needs.ChangeStatusAsync(id, NeedStatus.Resolved, Fixture.Actor)).Success);

        var details = await f.Needs.GetDetailsAsync(id);
        Assert.Equal(NeedStatus.Resolved, details!.Status);
        Assert.Equal(4, details.History.Count);   // ny + tre endringer, alle med tidspunkt
    }

    [Fact]
    public async Task ChangeStatus_SkippingSteps_IsRejected()
    {
        using var f = new Fixture();
        var id = await f.NeedAsync();

        var result = await f.Needs.ChangeStatusAsync(id, NeedStatus.Resolved, Fixture.Actor);

        Assert.False(result.Success);
        Assert.Equal(NeedStatus.New, (await f.Needs.GetDetailsAsync(id))!.Status);
    }

    [Fact]
    public async Task ChangeStatus_FromResolved_IsRejected()
    {
        using var f = new Fixture();
        var id = await f.NeedAsync();
        await f.Needs.ChangeStatusAsync(id, NeedStatus.UnderReview, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(id, NeedStatus.Assigned, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(id, NeedStatus.Resolved, Fixture.Actor);

        var result = await f.Needs.ChangeStatusAsync(id, NeedStatus.UnderReview, Fixture.Actor);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task List_HidesResolved_UnlessRequested_AndSortsAcuteFirst()
    {
        using var f = new Fixture();
        var planned = await f.Needs.CreateAsync(Fixture.Actor, new NeedInput("Transport", null, Fixture.Lat, Fixture.Lng,
            DateTime.UtcNow.AddDays(1), NeedPriority.Planned, "Kontakt"));
        var acute = await f.NeedAsync();
        var done = await f.NeedAsync();
        await f.Needs.ChangeStatusAsync(done, NeedStatus.UnderReview, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(done, NeedStatus.Assigned, Fixture.Actor);
        await f.Needs.ChangeStatusAsync(done, NeedStatus.Resolved, Fixture.Actor);

        var open = await f.Needs.ListAsync(includeResolved: false);
        var everything = await f.Needs.ListAsync(includeResolved: true);

        Assert.Equal(new[] { acute, planned }, open.Select(n => n.Id).ToArray());
        Assert.Equal(3, everything.Count);
    }

    [Fact]
    public async Task ChangeStatus_ToResolved_CompletesActiveAssignments_AndFreesResources()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);

        var result = await f.Needs.ChangeStatusAsync(need, NeedStatus.Resolved, Fixture.Actor);

        Assert.True(result.Success);
        var assignment = Assert.Single(f.Db.Assignments);
        Assert.Equal(MatchStatus.Completed, assignment.MatchStatus);
        Assert.Null(assignment.ActiveResourceKey);
        Assert.Single(await f.Resources.ListAvailableAsync());   // ressursen er ledig igjen
    }
}
