using FeedbackMap.Data;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Implementasjon av <see cref="IActorService"/>.</summary>
public class ActorService : IActorService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public ActorService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<ServiceResult> SetBaseAsync(string userId, decimal latitude, decimal longitude, string? label)
    {
        var profile = await _db.PublicActorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null) return ServiceResult.Deny();

        profile.BaseLatitude = latitude;
        profile.BaseLongitude = longitude;
        profile.BaseLabel = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        _audit.Add(userId, "ActorBaseSet", "PublicActor", userId, profile.BaseLabel);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Organisasjonens posisjon er lagret.");
    }

    public async Task<IReadOnlyList<ActorBase>> ListBasesAsync()
    {
        var list = await _db.PublicActorProfiles.AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.BaseLatitude != null && p.BaseLongitude != null)
            .ToListAsync();

        return list.Select(p => new ActorBase(p.UserId, p.Organisation, p.RoleTitle, p.BaseLabel,
            p.BaseLatitude!.Value, p.BaseLongitude!.Value, p.User.Email)).ToList();
    }
}
