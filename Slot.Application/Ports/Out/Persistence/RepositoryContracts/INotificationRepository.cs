namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetMyNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task<Notification?> FindAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task<Notification?> FindAsync(Guid notificationId, CancellationToken ct = default);
    Task CreateAsync(Notification notification, CancellationToken ct = default);
    Task UpdateAsync(Notification notification, CancellationToken ct = default);
    Task DeleteAsync(Notification notification, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
