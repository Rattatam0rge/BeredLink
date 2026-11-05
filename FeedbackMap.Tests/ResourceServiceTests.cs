using FeedbackMap.Models;
using FeedbackMap.Services;

namespace FeedbackMap.Tests;

public class ResourceServiceTests
{
    [Fact]
    public async Task Remove_WithoutAssignments_DeletesRow()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync();

        var result = await f.Resources.RemoveAsync(Fixture.Provider, id);

        Assert.True(result.Success);
        Assert.Empty(f.Db.Resources);
    }

    [Fact]
    public async Task Remove_WithHistory_DeactivatesInsteadOfDeleting()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var id = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, id, Fixture.Actor);
        await f.Assignments.UnassignAsync(f.Db.Assignments.Single().Id, Fixture.Actor);

        var result = await f.Resources.RemoveAsync(Fixture.Provider, id);

        Assert.True(result.Success);
        var resource = Assert.Single(f.Db.Resources);
        Assert.False(resource.IsActive);
        Assert.Empty(await f.Resources.ListForProviderAsync(Fixture.Provider));   // vises ikke lenger i listen
        Assert.Single(f.Db.Assignments);                                          // historikken er beholdt
    }

    [Fact]
    public async Task Remove_WhileAssigned_IsRejected()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var id = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, id, Fixture.Actor);

        var result = await f.Resources.RemoveAsync(Fixture.Provider, id);

        Assert.False(result.Success);
        Assert.True(Assert.Single(f.Db.Resources).IsActive);
    }

    [Fact]
    public async Task Remove_OthersResource_IsForbidden()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync(provider: Fixture.Provider);

        var result = await f.Resources.RemoveAsync(Fixture.Provider2, id);

        Assert.False(result.Success);
        Assert.True(result.Forbidden);
        Assert.Single(f.Db.Resources);
    }

    [Fact]
    public async Task ListForProvider_ShowsOnlyOwnResources()
    {
        using var f = new Fixture();
        await f.ResourceAsync(provider: Fixture.Provider);
        await f.ResourceAsync(provider: Fixture.Provider2);

        var own = await f.Resources.ListForProviderAsync(Fixture.Provider);

        Assert.Single(own);
    }

    [Fact]
    public async Task ListAvailable_ExcludesAssignedAndExpired()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var assigned = await f.ResourceAsync();
        await f.ResourceAsync(availableTo: DateTime.UtcNow.AddMinutes(-1));
        var free = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, assigned, Fixture.Actor);

        var available = await f.Resources.ListAvailableAsync();

        Assert.Equal(free, Assert.Single(available).Id);
    }

    [Fact]
    public async Task ListInBounds_ReturnsOnlyResourcesInsideRegion()
    {
        using var f = new Fixture();
        var inside = await f.ResourceAsync(lat: 58.15m, lng: 8.00m);
        await f.ResourceAsync(lat: 63.43m, lng: 10.39m);   // Trondheim

        var result = await f.Resources.ListInBoundsAsync(58.0m, 58.3m, 7.8m, 8.2m, type: null);

        Assert.Equal(inside, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetImage_ReturnsNull_WhenNoImage()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync();

        Assert.Null(await f.Resources.GetImageAsync(id));
    }

    private static ResourceUpdateInput Update(string type = "Traktor", string? newImage = null, bool removeImage = false) =>
        new(type, "Ny beskrivelse", Fixture.Lat + 0.01m, Fixture.Lng + 0.01m,
            DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddDays(3), newImage, removeImage);

    [Fact]
    public async Task Update_ChangesFields_ForOwner()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync("Aggregat");

        var result = await f.Resources.UpdateAsync(Fixture.Provider, id, Update("Traktor"));

        Assert.True(result.Success);
        var r = Assert.Single(f.Db.Resources);
        Assert.Equal("Traktor", r.Type);
        Assert.Equal("Ny beskrivelse", r.Description);
        Assert.Equal(Fixture.Lat + 0.01m, r.Latitude);
        Assert.NotNull(r.AvailableTo);
    }

    [Fact]
    public async Task Update_OthersResource_IsForbidden_AndUnchanged()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync("Aggregat", provider: Fixture.Provider);

        var result = await f.Resources.UpdateAsync(Fixture.Provider2, id, Update());

        Assert.True(result.Forbidden);
        Assert.Equal("Aggregat", Assert.Single(f.Db.Resources).Type);
    }

    [Fact]
    public async Task Update_WhileAssigned_IsRejected()
    {
        using var f = new Fixture();
        var need = await f.NeedAsync();
        var id = await f.ResourceAsync();
        await f.Assignments.AssignAsync(need, id, Fixture.Actor);

        var result = await f.Resources.UpdateAsync(Fixture.Provider, id, Update());

        Assert.False(result.Success);
        Assert.Equal("Aggregat", Assert.Single(f.Db.Resources).Type);
    }

    [Fact]
    public async Task Update_ReplacingImage_DeletesOldFile()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync();
        var oldFile = await SaveImage(f);
        var newFile = await SaveImage(f);
        f.Db.Resources.Single().ImageFileName = oldFile;
        await f.Db.SaveChangesAsync();

        await f.Resources.UpdateAsync(Fixture.Provider, id, Update(newImage: newFile));

        Assert.Equal(newFile, f.Db.Resources.Single().ImageFileName);
        Assert.Null(f.Images.Open(oldFile));
        Assert.NotNull(f.Images.Open(newFile));
    }

    [Fact]
    public async Task Update_RemoveImage_ClearsFieldAndDeletesFile()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync();
        var file = await SaveImage(f);
        f.Db.Resources.Single().ImageFileName = file;
        await f.Db.SaveChangesAsync();

        await f.Resources.UpdateAsync(Fixture.Provider, id, Update(removeImage: true));

        Assert.Null(f.Db.Resources.Single().ImageFileName);
        Assert.Null(f.Images.Open(file));
    }

    [Fact]
    public async Task GetForEdit_OnlyReturnsOwnResource()
    {
        using var f = new Fixture();
        var id = await f.ResourceAsync(provider: Fixture.Provider);

        Assert.NotNull(await f.Resources.GetForEditAsync(Fixture.Provider, id));
        Assert.Null(await f.Resources.GetForEditAsync(Fixture.Provider2, id));
    }

    private static async Task<string> SaveImage(Fixture f)
    {
        var bytes = new byte[100];
        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }.CopyTo(bytes, 0);
        var stream = new MemoryStream(bytes);
        var file = new Microsoft.AspNetCore.Http.FormFile(stream, 0, stream.Length, "Image", "x.jpg");
        var saved = await f.Images.SaveAsync(file);
        return saved.FileName!;
    }
}
