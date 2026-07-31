namespace Slot.Adapters.PostgreSql.Repositories;

public class GroundRepository(ApplicationDbContext db) : IGroundRepository
{
    public async Task<IReadOnlyList<Ground>> GetAllAsync(GroundListRequest request, CancellationToken ct = default)
    {
        var query = BuildBaseQuery();
        query = ApplyFilters(query, request);
        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync(ct);
    }

    public async Task<Ground?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await BuildBaseQuery().FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<IReadOnlyList<Ground>> FindByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await BuildBaseQuery()
            .Where(g => g.OwnerId == ownerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Ground ground, CancellationToken ct = default)
    {
        await db.Grounds.AddAsync(ground, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Ground ground, CancellationToken ct = default)
    {
        db.Grounds.Update(ground);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Ground ground, CancellationToken ct = default)
    {
        db.Grounds.Remove(ground);
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Ground> BuildBaseQuery() => db.Grounds
        .AsNoTracking()
        .Include(g => g.Images)
        .Include(g => g.Schedules)
        .Include(g => g.Sports)
            .ThenInclude(gs => gs.Sport);

    private static IQueryable<Ground> ApplyFilters(IQueryable<Ground> query, GroundListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(g =>
                (g.Name != null && g.Name.ToLower().Contains(search)) ||
                (g.Description != null && g.Description.ToLower().Contains(search)) ||
                (g.Address != null && g.Address.ToLower().Contains(search)));
        }

        if (request.SportId.HasValue)
            query = query.Where(g => g.Sports.Any(s => s.SportId == request.SportId.Value));

        if (request.MinHourlyRate.HasValue)
            query = query.Where(g => g.HourlyRate >= request.MinHourlyRate.Value);

        if (request.MaxHourlyRate.HasValue)
            query = query.Where(g => g.HourlyRate <= request.MaxHourlyRate.Value);

        if (request.Latitude.HasValue && request.Longitude.HasValue && request.RadiusKm.HasValue && request.RadiusKm.Value > 0)
        {
            var latDelta = request.RadiusKm.Value / 111.32m;
            var lngScale = Math.Cos((double)request.Latitude.Value * Math.PI / 180.0);
            var lngDelta = lngScale == 0 ? request.RadiusKm.Value / 111.32m : request.RadiusKm.Value / (111.32m * (decimal)lngScale);

            var minLat = request.Latitude.Value - latDelta;
            var maxLat = request.Latitude.Value + latDelta;
            var minLng = request.Longitude.Value - lngDelta;
            var maxLng = request.Longitude.Value + lngDelta;

            query = query.Where(g =>
                g.Latitude >= minLat && g.Latitude <= maxLat &&
                g.Longitude >= minLng && g.Longitude <= maxLng);
        }

        return query;
    }
}
