using FeedbackMap.Data;
using FeedbackMap.Models;
using Microsoft.EntityFrameworkCore;

namespace FeedbackMap.Services;

/// <summary>Implementasjon av <see cref="IResourceService"/> mot databasen og bildelageret.</summary>
public class ResourceService : IResourceService
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorage _images;
    private readonly IAuditService _audit;

    public ResourceService(ApplicationDbContext db, IImageStorage images, IAuditService audit)
    {
        _db = db;
        _images = images;
        _audit = audit;
    }

    public async Task<int> CreateAsync(string userId, ResourceInput input)
    {
        var resource = new Resource
        {
            ResourceProviderId = userId,
            Type = input.Type,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            AvailableFrom = input.AvailableFrom,
            AvailableTo = input.AvailableTo,
            ImageFileName = input.ImageFileName,
            Status = ResourceStatus.Available,
            IsActive = true
        };
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(userId, "ResourceCreated", "Resource", resource.Id.ToString(),
            input.ImageFileName is null ? resource.Type : resource.Type + " (med bilde)");
        return resource.Id;
    }

    public async Task<ResourceEditData?> GetForEditAsync(string userId, int id)
    {
        var r = await _db.Resources.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.ResourceProviderId == userId);
        return r is null
            ? null
            : new ResourceEditData(r.Id, r.Type, r.Description, r.Latitude, r.Longitude,
                r.AvailableFrom, r.AvailableTo, r.ImageFileName is not null);
    }

    public async Task<ServiceResult> UpdateAsync(string userId, int id, ResourceUpdateInput input)
    {
        var r = await _db.Resources.Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (r is null) return ServiceResult.Fail("Fant ikke ressursen.");
        if (r.ResourceProviderId != userId) return ServiceResult.Deny();
        if (r.Assignments.Any(a => a.ActiveResourceKey != null))
            return ServiceResult.Fail("Ressursen er tildelt et behov akkurat nå og kan ikke endres før tildelingen er avsluttet.");

        var oldImage = r.ImageFileName;

        r.Type = input.Type;
        r.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        r.Latitude = input.Latitude;
        r.Longitude = input.Longitude;
        r.AvailableFrom = input.AvailableFrom;
        r.AvailableTo = input.AvailableTo;

        if (input.NewImageFileName is not null) r.ImageFileName = input.NewImageFileName;
        else if (input.RemoveImage) r.ImageFileName = null;

        _audit.Add(userId, "ResourceUpdated", "Resource", r.Id.ToString(), r.Type);
        await _db.SaveChangesAsync();

        // Gammelt bilde ryddes bort først når endringen er lagret
        if (oldImage is not null && oldImage != r.ImageFileName) _images.Delete(oldImage);
        return ServiceResult.Ok("Ressursen er oppdatert.");
    }

    public async Task<IReadOnlyList<ResourceListItem>> ListForProviderAsync(string userId)
    {
        var list = await Query()
            .Where(r => r.ResourceProviderId == userId && r.IsActive)
            .OrderBy(r => r.Type)
            .ToListAsync();
        return list.Select(ToItem).ToList();
    }

    public async Task<IReadOnlyList<ResourceListItem>> ListAvailableAsync()
    {
        var now = DateTime.UtcNow;
        var list = await Query()
            .Where(r => r.IsActive
                        && r.Status == ResourceStatus.Available
                        && (r.AvailableTo == null || r.AvailableTo >= now)
                        && !r.Assignments.Any(a => a.ActiveResourceKey != null))
            .OrderBy(r => r.Type)
            .ToListAsync();
        return list.Select(ToItem).ToList();
    }

    public async Task<IReadOnlyList<ResourceListItem>> ListInBoundsAsync(
        decimal minLat, decimal maxLat, decimal minLng, decimal maxLng, string? type)
    {
        var now = DateTime.UtcNow;
        var query = Query().Where(r => r.IsActive
                                       && r.Status == ResourceStatus.Available
                                       && (r.AvailableTo == null || r.AvailableTo >= now)
                                       && r.Latitude >= minLat && r.Latitude <= maxLat
                                       && r.Longitude >= minLng && r.Longitude <= maxLng);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(r => r.Type == type);

        var list = await query.OrderBy(r => r.Type).ThenBy(r => r.Latitude).ToListAsync();
        return list.Select(ToItem).ToList();
    }

    public async Task<ResourceImageInfo?> GetImageAsync(int id)
    {
        var row = await _db.Resources.AsNoTracking()
            .Where(r => r.Id == id && r.IsActive && r.ImageFileName != null)
            .Select(r => new { r.ResourceProviderId, r.ImageFileName })
            .FirstOrDefaultAsync();
        return row is null ? null : new ResourceImageInfo(row.ResourceProviderId, row.ImageFileName!);
    }

    public async Task<ServiceResult> RemoveAsync(string userId, int id)
    {
        var resource = await _db.Resources.Include(r => r.Assignments)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        if (resource is null) return ServiceResult.Fail("Fant ikke ressursen.");
        if (resource.ResourceProviderId != userId) return ServiceResult.Deny();

        if (resource.Assignments.Count > 0)
        {
            if (resource.Assignments.Any(a => a.ActiveResourceKey != null))
                return ServiceResult.Fail("Ressursen er tildelt et behov akkurat nå. Kontakt kriseledelsen først.");

            // Har historikk: behold raden slik at tidligere tildelinger ikke forsvinner
            resource.IsActive = false;
            resource.Status = ResourceStatus.Unavailable;
            _audit.Add(userId, "ResourceDeactivated", "Resource", resource.Id.ToString(), resource.Type);
            await _db.SaveChangesAsync();
            return ServiceResult.Ok("Ressursen er fjernet fra listen. Historikken er beholdt.");
        }

        var imageFile = resource.ImageFileName;
        _db.Resources.Remove(resource);
        _audit.Add(userId, "ResourceDeleted", "Resource", resource.Id.ToString(), resource.Type);
        await _db.SaveChangesAsync();
        if (imageFile is not null) _images.Delete(imageFile);
        return ServiceResult.Ok("Ressursen er slettet.");
    }

    private IQueryable<Resource> Query() =>
        _db.Resources.AsNoTracking()
            .Include(r => r.Assignments)
            .Include(r => r.ResourceProvider).ThenInclude(p => p.User);

    internal static ResourceListItem ToItem(Resource r) =>
        new(r.Id, r.Type, r.Description, r.Latitude, r.Longitude, r.AvailableFrom, r.AvailableTo, r.Status,
            r.ResourceProvider.ProviderType, r.ResourceProvider.ContactPhone, r.ResourceProvider.User.Email,
            r.ImageFileName is not null, r.Assignments.Any(a => a.ActiveResourceKey != null));
}
