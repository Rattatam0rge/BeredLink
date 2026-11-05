using FeedbackMap.Data;
using FeedbackMap.Models;
using FeedbackMap.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FeedbackMap.Tests;

/// <summary>Setter opp en ren in-memory database og alle services for én test.</summary>
public sealed class Fixture : IDisposable
{
    public const string Actor = "actor1";
    public const string Provider = "prov1";
    public const string Provider2 = "prov2";

    // Kristiansand sentrum
    public const decimal Lat = 58.1467m;
    public const decimal Lng = 7.9956m;

    public ApplicationDbContext Db { get; }
    public AuditService Audit { get; }
    public ImageStorage Images { get; }
    public NeedService Needs { get; }
    public ResourceService Resources { get; }
    public AssignmentService Assignments { get; }
    public NotificationService Notifications { get; }
    public string TempDir { get; }

    public Fixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Db = new ApplicationDbContext(options);

        TempDir = Path.Combine(Path.GetTempPath(), "feedbackmap-tests-" + Guid.NewGuid().ToString("N"));
        Audit = new AuditService(Db);
        Images = new ImageStorage(Options.Create(new StorageOptions { UploadsPath = TempDir }));
        Needs = new NeedService(Db, Audit);
        Resources = new ResourceService(Db, Images, Audit);
        Assignments = new AssignmentService(Db, Audit);
        Notifications = new NotificationService(Db);

        Db.Users.AddRange(
            new ApplicationUser { Id = Actor, UserName = "actor@test.no", Email = "actor@test.no" },
            new ApplicationUser { Id = Provider, UserName = "prov1@test.no", Email = "prov1@test.no" },
            new ApplicationUser { Id = Provider2, UserName = "prov2@test.no", Email = "prov2@test.no" });
        Db.PublicActorProfiles.Add(new PublicActorProfile { UserId = Actor, Organisation = "Test kommune" });
        Db.ResourceProviderProfiles.AddRange(
            new ResourceProviderProfile { UserId = Provider, ProviderType = ProviderType.Farmer, ContactPhone = "11111111" },
            new ResourceProviderProfile { UserId = Provider2, ProviderType = ProviderType.Business, ContactPhone = "22222222" });
        Db.SaveChanges();
    }

    public Task<int> NeedAsync(string type = "Strøm/aggregat", decimal lat = Lat, decimal lng = Lng) =>
        Needs.CreateAsync(Actor, new NeedInput(type, null, lat, lng, DateTime.UtcNow.AddDays(1),
            NeedPriority.Acute, "Kari Nordmann 12345678"));

    public Task<int> ResourceAsync(string type = "Aggregat", decimal lat = Lat, decimal lng = Lng,
        string provider = Provider, DateTime? availableTo = null) =>
        Resources.CreateAsync(provider, new ResourceInput(type, null, lat, lng,
            DateTime.UtcNow.AddHours(-1), availableTo));

    public void Dispose()
    {
        Db.Dispose();
        if (Directory.Exists(TempDir)) Directory.Delete(TempDir, recursive: true);
    }
}
