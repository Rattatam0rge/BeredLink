using FeedbackMap.Data;
using FeedbackMap.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Implementasjon av <see cref="INeedService"/>, inkludert statusflyten.</summary>
public class NeedService : INeedService
{
    // Statusflyt (user story #8): ny -> vurderes -> tildelt -> løst. Fra tildelt kan man gå tilbake til vurderes.
    private static readonly Dictionary<NeedStatus, NeedStatus[]> Flow = new()
    {
        [NeedStatus.New] = new[] { NeedStatus.UnderReview },
        [NeedStatus.UnderReview] = new[] { NeedStatus.Assigned },
        [NeedStatus.Assigned] = new[] { NeedStatus.Resolved, NeedStatus.UnderReview },
        [NeedStatus.Resolved] = Array.Empty<NeedStatus>()
    };

    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public NeedService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<int> CreateAsync(string userId, NeedInput input)
    {
        var need = new Need
        {
            PublicActorId = userId,
            Type = input.Type,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            Deadline = input.Deadline,
            Priority = input.Priority,
            ContactPoint = input.ContactPoint.Trim()
        };

        // "new" logges automatisk ved opprettelse (acceptance criteria #8)
        need.StatusLog.Add(new NeedStatusLog { Status = NeedStatus.New, ChangedByActorId = userId });

        _db.Needs.Add(need);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId, "NeedCreated", "Need", need.Id.ToString(), $"{need.Type}, {need.Priority}");
        return need.Id;
    }

    public async Task<IReadOnlyList<NeedListItem>> ListAsync(bool includeResolved)
    {
        var needs = await _db.Needs.AsNoTracking()
            .Include(n => n.StatusLog)
            .Include(n => n.PublicActor)
            .ToListAsync();

        return needs
            .Select(n => new NeedListItem(n.Id, n.Type, n.Priority, n.CurrentStatus, n.Deadline,
                n.PublicActor.Organisation, n.Latitude, n.Longitude, n.ContactPoint))
            .Where(n => includeResolved || n.Status != NeedStatus.Resolved)
            .OrderBy(n => n.Priority)      // akutt først
            .ThenBy(n => n.Deadline)
            .ToList();
    }

    public async Task<NeedDetails?> GetDetailsAsync(int id)
    {
        var n = await _db.Needs.AsNoTracking()
            .Include(x => x.PublicActor)
            .Include(x => x.StatusLog).ThenInclude(s => s.ChangedByActor)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (n is null) return null;

        var history = n.StatusLog
            .OrderByDescending(s => s.ChangedAt).ThenByDescending(s => s.Id)
            .Select(s => new StatusHistoryItem(s.Status, s.ChangedAt, s.ChangedByActor.Email ?? "ukjent"))
            .ToList();

        var current = n.CurrentStatus;
        return new NeedDetails(n.Id, n.Type, n.Description, n.Priority, current, n.Deadline, n.ContactPoint,
            n.Latitude, n.Longitude, n.PublicActor.Organisation, n.CreatedAt, history, Flow[current]);
    }

    public async Task<ServiceResult> ChangeStatusAsync(int id, NeedStatus newStatus, string userId)
    {
        var need = await _db.Needs
            .Include(n => n.StatusLog)
            .Include(n => n.Assignments)
            .FirstOrDefaultAsync(n => n.Id == id);
        if (need is null) return ServiceResult.Fail("Fant ikke behovet.");

        var current = need.CurrentStatus;
        if (!Flow[current].Contains(newStatus))
            return ServiceResult.Fail(
                $"Status kan ikke endres fra «{Labels.Status(current)}» til «{Labels.Status(newStatus)}».");

        var now = DateTime.UtcNow;

        // Når behovet er løst, fullføres aktive tildelinger og ressursene blir ledige igjen
        if (newStatus == NeedStatus.Resolved)
        {
            foreach (var a in need.Assignments.Where(a => a.ActiveResourceKey != null))
            {
                a.MatchStatus = MatchStatus.Completed;
                a.ActiveResourceKey = null;
                a.UnassignedAt = now;
            }
        }

        need.StatusLog.Add(new NeedStatusLog
        {
            NeedId = need.Id,
            Status = newStatus,
            ChangedByActorId = userId,
            ChangedAt = now
        });
        _audit.Add(userId, "NeedStatusChanged", "Need", need.Id.ToString(), $"{current} -> {newStatus}");
        await _db.SaveChangesAsync();
        return ServiceResult.Ok($"Status er endret til «{Labels.Status(newStatus)}».");
    }
}
