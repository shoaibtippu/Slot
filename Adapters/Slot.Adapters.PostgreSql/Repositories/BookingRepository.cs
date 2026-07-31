namespace Slot.Adapters.PostgreSql.Repositories;

public class BookingRepository(ApplicationDbContext db) : IBookingRepository
{
    public async Task<IReadOnlyList<Booking>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Ground)
            .Include(b => b.User)
                .ThenInclude(u => u.UserIdentity)
            .Include(b => b.Payments)
            .Where(b => b.UserId == userId || b.Ground.OwnerId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync(ct);
    }

    public async Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Booking?> FindByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Ground)
            .Include(b => b.User)
                .ThenInclude(u => u.UserIdentity)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Booking>> GetByGroundAndDateAsync(Guid groundId, DateOnly date, CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Where(b => b.GroundId == groundId && b.BookingDate == date)
            .OrderBy(b => b.StartTime)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Bookings.AsNoTracking().AnyAsync(b => b.Id == id, ct);
    }

    public async Task CreateAsync(Booking booking, CancellationToken ct = default)
    {
        await db.Bookings.AddAsync(booking, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        db.Bookings.Update(booking);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Booking booking, CancellationToken ct = default)
    {
        db.Bookings.Remove(booking);
        await db.SaveChangesAsync(ct);
    }
}
