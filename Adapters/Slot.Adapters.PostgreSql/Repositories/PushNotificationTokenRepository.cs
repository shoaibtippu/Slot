namespace Slot.Adapters.PostgreSql.Repositories;

public class PushNotificationTokenRepository(ApplicationDbContext db) : IPushNotificationTokenRepository
{
    public async Task<PushNotificationToken?> FindAsync(Guid userId, string token, CancellationToken ct = default)
    {
        return await db.Set<PushNotificationToken>().FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, ct);
    }

    public async Task CreateAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default)
    {
        await db.Set<PushNotificationToken>().AddAsync(pushNotificationToken, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default)
    {
        db.Set<PushNotificationToken>().Update(pushNotificationToken);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(PushNotificationToken pushNotificationToken, CancellationToken ct = default)
    {
        db.Set<PushNotificationToken>().Remove(pushNotificationToken);
        await db.SaveChangesAsync(ct);
    }
}
