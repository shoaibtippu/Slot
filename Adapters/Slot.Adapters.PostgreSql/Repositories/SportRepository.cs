namespace Slot.Adapters.PostgreSql.Repositories;

public class SportRepository(ApplicationDbContext db) : ISportRepository
{
    public async Task<IReadOnlyList<Sport>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Sports
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Sport?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Sports
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLower();

        return await db.Sports
            .AsNoTracking()
            .AnyAsync(s => s.Name.ToLower() == normalized && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
    }

    public async Task CreateAsync(Sport sport, CancellationToken ct = default)
    {
        await db.Sports.AddAsync(sport, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Sport sport, CancellationToken ct = default)
    {
        db.Sports.Update(sport);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Sport sport, CancellationToken ct = default)
    {
        db.Sports.Remove(sport);
        await db.SaveChangesAsync(ct);
    }
}