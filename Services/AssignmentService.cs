using FeedbackMap.Data;
using FeedbackMap.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Matching og tildeling. En unik indeks på <c>ActiveResourceKey</c> sørger for at en ressurs aldri har to aktive tildelinger.</summary>
public class AssignmentService : IAssignmentService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public AssignmentService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<NearbyResource>> FindNearbyAsync(int needId, double radiusMeters, bool onlySuitableTypes)
    {
        var need = await _db.Needs.AsNoTracking().FirstOrDefaultAsync(n => n.Id == needId);
        if (need is null) return Array.Empty<NearbyResource>();

        double lat = (double)need.Latitude, lng = (double)need.Longitude;

        // Grov filtrering i databasen (bounding box), nøyaktig avstand beregnes i minnet
        var dLat = radiusMeters / 111_320.0;
        var dLng = radiusMeters / (111_320.0 * Math.Max(0.01, Math.Cos(lat * Math.PI / 180)));
        var minLat = (decimal)(lat - dLat);
        var maxLat = (decimal)(lat + dLat);
        var minLng = (decimal)(lng - dLng);
        var maxLng = (decimal)(lng + dLng);
        var now = DateTime.UtcNow;

        var candidates = await _db.Resources.AsNoTracking()
            .Include(r => r.Assignments)
            .Include(r => r.ResourceProvider).ThenInclude(p => p.User)
            .Where(r => r.IsActive
                        && r.Status == ResourceStatus.Available
                        && (r.AvailableTo == null || r.AvailableTo >= now)
                        && r.Latitude >= minLat && r.Latitude <= maxLat
                        && r.Longitude >= minLng && r.Longitude <= maxLng
                        && !r.Assignments.Any(a => a.ActiveResourceKey != null))
            .ToListAsync();

        return candidates
            .Select(r => new NearbyResource(
                ResourceService.ToItem(r),
                Geo.DistanceMeters(lat, lng, (double)r.Latitude, (double)r.Longitude),
                Catalog.IsSuitable(need.Type, r.Type)))
            .Where(x => x.DistanceMeters <= radiusMeters && (!onlySuitableTypes || x.TypeMatches))
            .OrderBy(x => x.DistanceMeters)
            .Take(50)
            .ToList();
    }

    public async Task<ServiceResult> AssignAsync(int needId, int resourceId, string userId)
    {
        var need = await _db.Needs.Include(n => n.StatusLog).FirstOrDefaultAsync(n => n.Id == needId);
        if (need is null) return ServiceResult.Fail("Fant ikke behovet.");

        var current = need.CurrentStatus;
        if (current == NeedStatus.Resolved) return ServiceResult.Fail("Behovet er allerede løst.");

        var resource = await _db.Resources.Include(r => r.Assignments)
            .FirstOrDefaultAsync(r => r.Id == resourceId);
        var now = DateTime.UtcNow;

        if (resource is null || !resource.IsActive) return ServiceResult.Fail("Fant ikke ressursen.");
        if (resource.Status != ResourceStatus.Available || (resource.AvailableTo != null && resource.AvailableTo < now))
            return ServiceResult.Fail("Ressursen er ikke tilgjengelig.");
        if (resource.Assignments.Any(a => a.ActiveResourceKey != null))
            return ServiceResult.Fail("Ressursen er allerede tildelt et behov.");

        var assignment = new Assignment
        {
            NeedId = need.Id,
            ResourceId = resource.Id,
            AssignedAt = now,
            MatchStatus = MatchStatus.Confirmed,
            ActiveResourceKey = resource.Id,
            Notification = new Notification
            {
                Message = BuildMessage(need, resource),
                CreatedAt = now,
                IsRead = false
            }
        };
        _db.Assignments.Add(assignment);

        // Følg statusflyten: ny -> vurderes -> tildelt
        if (current == NeedStatus.New) AddStatus(need, NeedStatus.UnderReview, userId, now);
        if (current != NeedStatus.Assigned) AddStatus(need, NeedStatus.Assigned, userId, now.AddMilliseconds(1));

        _audit.Add(userId, "ResourceAssigned", "Need", need.Id.ToString(), $"Ressurs {resource.Id} ({resource.Type})");

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Unik indeks på ActiveResourceKey: noen andre tildelte ressursen samtidig
            return ServiceResult.Fail("Ressursen ble tildelt av noen andre akkurat nå.");
        }

        return ServiceResult.Ok("Ressursen er tildelt, og leverandøren er varslet.");
    }

    public async Task<ServiceResult> UnassignAsync(int assignmentId, string userId)
    {
        var a = await _db.Assignments
            .Include(x => x.Need).ThenInclude(n => n.StatusLog)
            .Include(x => x.Resource)
            .Include(x => x.Notification)
            .FirstOrDefaultAsync(x => x.Id == assignmentId);

        if (a is null) return ServiceResult.Fail("Fant ikke tildelingen.");
        if (a.ActiveResourceKey is null) return ServiceResult.Fail("Tildelingen er ikke aktiv.");

        var now = DateTime.UtcNow;
        a.ActiveResourceKey = null;
        a.MatchStatus = MatchStatus.Cancelled;
        a.UnassignedAt = now;

        if (a.Notification is not null)
        {
            a.Notification.Message =
                $"Tildelingen av «{a.Resource.Type}» til behovet «{a.Need.Type}» er avbrutt. Ressursen din er ledig igjen.";
            a.Notification.CreatedAt = now;
            a.Notification.IsRead = false;
        }

        var otherActive = await _db.Assignments.AnyAsync(x =>
            x.NeedId == a.NeedId && x.Id != a.Id && x.ActiveResourceKey != null);
        if (!otherActive && a.Need.CurrentStatus == NeedStatus.Assigned)
            AddStatus(a.Need, NeedStatus.UnderReview, userId, now);

        _audit.Add(userId, "AssignmentCancelled", "Need", a.NeedId.ToString(), $"Ressurs {a.ResourceId}");
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Tildelingen er avbrutt.");
    }

    public async Task<IReadOnlyList<AssignmentView>> ListForNeedAsync(int needId)
    {
        var list = await _db.Assignments.AsNoTracking()
            .Include(a => a.Resource).ThenInclude(r => r.ResourceProvider).ThenInclude(p => p.User)
            .Where(a => a.NeedId == needId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        return list.Select(a => new AssignmentView(
            a.Id, a.ResourceId, a.Resource.Type,
            a.Resource.ResourceProvider.ContactPhone, a.Resource.ResourceProvider.User.Email,
            a.AssignedAt, a.UnassignedAt, a.MatchStatus, a.ActiveResourceKey != null)).ToList();
    }

    private static void AddStatus(Need need, NeedStatus status, string userId, DateTime at) =>
        need.StatusLog.Add(new NeedStatusLog
        {
            NeedId = need.Id,
            Status = status,
            ChangedByActorId = userId,
            ChangedAt = at
        });

    private static string BuildMessage(Need need, Resource resource)
    {
        var msg = $"Ressursen din «{resource.Type}» er tildelt behovet «{need.Type}». " +
                  $"Sted: {need.Latitude:F4}, {need.Longitude:F4}. Frist: {Labels.Date(need.Deadline)}. " +
                  $"Kontaktperson: {need.ContactPoint}.";
        return msg.Length > 500 ? msg[..500] : msg;
    }
}
