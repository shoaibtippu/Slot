namespace Slot.Adapters.PostgreSql.Repositories;

public class FavoriteRepository(ApplicationDbContext db) : IFavoriteRepository
{
    public async Task<bool> ExistsAsync(Guid userId, Guid groundId, CancellationToken ct = default)
    {
        return await db.FavoriteGrounds.AsNoTracking().AnyAsync(fg => fg.UserId == userId && fg.GroundId == groundId, ct);
    }

    public async Task CreateAsync(FavoriteGround favoriteGround, CancellationToken ct = default)
    {
        await db.FavoriteGrounds.AddAsync(favoriteGround, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(FavoriteGround favoriteGround, CancellationToken ct = default)
    {
        db.FavoriteGrounds.Remove(favoriteGround);
        await db.SaveChangesAsync(ct);
    }

    public async Task<FavoriteGround?> FindAsync(Guid userId, Guid groundId, CancellationToken ct = default)
    {
        return await db.FavoriteGrounds.FirstOrDefaultAsync(fg => fg.UserId == userId && fg.GroundId == groundId, ct);
    }

    public async Task<IReadOnlyList<Ground>> GetMyFavoriteGroundsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.FavoriteGrounds
            .AsNoTracking()
            .Where(fg => fg.UserId == userId)
            .Select(fg => fg.Ground)
            .Include(g => g.Images)
            .Include(g => g.Sports)
                .ThenInclude(gs => gs.Sport)
            .Include(g => g.Schedules)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }
}
