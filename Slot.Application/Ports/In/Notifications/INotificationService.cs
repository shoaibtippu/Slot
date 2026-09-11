namespace Slot.Application.Ports.In.Notifications;

public interface INotificationService
{
    Task<Result<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(string userIdentityId, Guid notificationId, CancellationToken ct = default);
    Task<Result> MarkAllAsReadAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userIdentityId, Guid notificationId, CancellationToken ct = default);
    Task<Result> RegisterPushTokenAsync(string userIdentityId, RegisterPushTokenRequest request, CancellationToken ct = default);
    Task<Result> UnregisterPushTokenAsync(string userIdentityId, UnregisterPushTokenRequest request, CancellationToken ct = default);
    Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default);
}
