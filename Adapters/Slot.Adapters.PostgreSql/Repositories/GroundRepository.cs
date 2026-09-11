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

    public async Task<IReadOnlyList<GroundImage>> GetImagesAsync(Guid groundId, CancellationToken ct = default)
    {
        return await db.GroundImages
            .AsNoTracking()
            .Where(i => i.GroundId == groundId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<GroundImage?> FindImageAsync(Guid groundId, Guid imageId, CancellationToken ct = default)
    {
        return await db.GroundImages
            .FirstOrDefaultAsync(i => i.GroundId == groundId && i.Id == imageId, ct);
    }

    public async Task<IReadOnlyList<GroundSchedule>> GetSchedulesAsync(Guid groundId, CancellationToken ct = default)
    {
        return await db.GroundSchedules
            .AsNoTracking()
            .Where(s => s.GroundId == groundId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(ct);
    }

    public async Task ReplaceSchedulesAsync(Guid groundId, IEnumerable<GroundSchedule> schedules, CancellationToken ct = default)
    {
        var existing = db.GroundSchedules.Where(s => s.GroundId == groundId);
        db.GroundSchedules.RemoveRange(existing);
        await db.GroundSchedules.AddRangeAsync(schedules, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<GroundAvailability>> GetAvailabilityBlocksAsync(Guid groundId, DateOnly date, CancellationToken ct = default)
    {
        return await db.GroundAvailabilities
            .AsNoTracking()
            .Where(a => a.GroundId == groundId && a.Date == date && a.IsBlocked)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);
    }

    public async Task<GroundAvailability?> FindAvailabilityBlockAsync(Guid groundId, Guid blockId, CancellationToken ct = default)
    {
        return await db.GroundAvailabilities
            .FirstOrDefaultAsync(a => a.GroundId == groundId && a.Id == blockId && a.IsBlocked, ct);
    }

    public async Task AddAvailabilityBlockAsync(GroundAvailability block, CancellationToken ct = default)
    {
        await db.GroundAvailabilities.AddAsync(block, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAvailabilityBlockAsync(GroundAvailability block, CancellationToken ct = default)
    {
        db.GroundAvailabilities.Remove(block);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsAsync(Guid groundId, DateOnly date, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Where(b => b.GroundId == groundId
                && b.BookingDate == date
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved || b.Status == BookingStatus.Completed))
            .OrderBy(b => b.StartTime)
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

    public async Task AddImagesAsync(IEnumerable<GroundImage> images, CancellationToken ct = default)
    {
        await db.GroundImages.AddRangeAsync(images, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateImagesAsync(IEnumerable<GroundImage> images, CancellationToken ct = default)
    {
        db.GroundImages.UpdateRange(images);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReplaceImagesAsync(Guid groundId, IEnumerable<GroundImage> images, CancellationToken ct = default)
    {
        var existing = db.GroundImages.Where(i => i.GroundId == groundId);
        db.GroundImages.RemoveRange(existing);
        await db.GroundImages.AddRangeAsync(images, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Ground ground, CancellationToken ct = default)
    {
        db.Grounds.Remove(ground);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteImageAsync(GroundImage image, CancellationToken ct = default)
    {
        db.GroundImages.Remove(image);
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
