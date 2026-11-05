using FeedbackMap.Services;

namespace FeedbackMap.Tests;

public class NotificationAndAuditTests
{
    [Fact]
    public async Task MarkRead_OnlyWorksForOwner()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync(provider: Fixture.Provider);
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);
        var notification = (await f.Notifications.ListAsync(Fixture.Provider)).Single();

        Assert.False(await f.Notifications.MarkReadAsync(Fixture.Provider2, notification.Id));   // feil bruker
        Assert.Equal(1, await f.Notifications.UnreadCountAsync(Fixture.Provider));

        Assert.True(await f.Notifications.MarkReadAsync(Fixture.Provider, notification.Id));
        Assert.Equal(0, await f.Notifications.UnreadCountAsync(Fixture.Provider));
    }

    [Fact]
    public async Task OtherProviders_DoNotSeeNotifications()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync(provider: Fixture.Provider);
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);

        Assert.Empty(await f.Notifications.ListAsync(Fixture.Provider2));
    }

    [Fact]
    public async Task MarkAllRead_ClearsUnreadCount()
    {
        using var f = new Fixture();
        var need1 = await f.NeedAsync();
        var need2 = await f.NeedAsync();
        await f.Assignments.AssignAsync(need1, await f.ResourceAsync(), Fixture.Actor);
        await f.Assignments.AssignAsync(need2, await f.ResourceAsync(), Fixture.Actor);

        await f.Notifications.MarkAllReadAsync(Fixture.Provider);

        Assert.Equal(0, await f.Notifications.UnreadCountAsync(Fixture.Provider));
    }

    [Fact]
    public async Task ImportantActions_AreWrittenToAuditLog()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var resource = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, resource, Fixture.Actor);

        var log = await f.Audit.ListRecentAsync();
        var actions = log.Select(l => l.Action).ToList();

        Assert.Contains("NeedCreated", actions);
        Assert.Contains("ResourceCreated", actions);
        Assert.Contains("ResourceAssigned", actions);
        Assert.Contains(log, l => l.User == "actor@test.no");   // hvem gjorde det
    }
}
