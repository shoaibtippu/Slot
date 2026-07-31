namespace Slot.Adapters.PostgreSql.Repositories;

public class NotificationRepository(ApplicationDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetMyNotificationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Notification?> FindAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        return await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
    }

    public async Task<Notification?> FindAsync(Guid notificationId, CancellationToken ct = default)
    {
        return await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, ct);
    }

    public async Task CreateAsync(Notification notification, CancellationToken ct = default)
    {
        await db.Notifications.AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Remove(notification);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        foreach (var notification in notifications)
            notification.IsRead = true;

        if (notifications.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
