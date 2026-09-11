namespace Slot.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IPushNotificationTokenRepository pushTokenRepository,
    IUserRepository userRepository) : INotificationService
{
    public async Task<Result<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<NotificationResponse>>(Error.NotFound("User not found."));

        var notifications = await notificationRepository.GetMyNotificationsAsync(profile.Id, ct);
        return Result.Success<IReadOnlyList<NotificationResponse>>(notifications.Select(Map).ToList());
    }

    public async Task<Result> MarkAsReadAsync(string userIdentityId, Guid notificationId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var notification = await notificationRepository.FindAsync(notificationId, profile.Id, ct);
        if (notification is null)
            return Result.Failure(Error.NotFound("Notification not found."));

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await notificationRepository.UpdateAsync(notification, ct);
        }

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        await notificationRepository.MarkAllAsReadAsync(profile.Id, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(string userIdentityId, Guid notificationId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var notification = await notificationRepository.FindAsync(notificationId, profile.Id, ct);
        if (notification is null)
            return Result.Failure(Error.NotFound("Notification not found."));

        await notificationRepository.DeleteAsync(notification, ct);
        return Result.Success();
    }

    public async Task<Result> RegisterPushTokenAsync(string userIdentityId, RegisterPushTokenRequest request, CancellationToken ct = default)
    {
        var token = request.Token.Trim();
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure(Error.Validation("Token is required."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var existing = await pushTokenRepository.FindAsync(profile.Id, token, ct);
        if (existing is null)
        {
            await pushTokenRepository.CreateAsync(new PushNotificationToken
            {
                Id = Guid.NewGuid(),
                UserId = profile.Id,
                Token = token,
                Platform = string.IsNullOrWhiteSpace(request.Platform) ? null : request.Platform.Trim()
            }, ct);
            return Result.Success();
        }

        existing.Platform = string.IsNullOrWhiteSpace(request.Platform) ? existing.Platform : request.Platform.Trim();
        await pushTokenRepository.UpdateAsync(existing, ct);
        return Result.Success();
    }

    public async Task<Result> UnregisterPushTokenAsync(string userIdentityId, UnregisterPushTokenRequest request, CancellationToken ct = default)
    {
        var token = request.Token.Trim();
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure(Error.Validation("Token is required."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var existing = await pushTokenRepository.FindAsync(profile.Id, token, ct);
        if (existing is null)
            return Result.Success();

        await pushTokenRepository.DeleteAsync(existing, ct);
        return Result.Success();
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false
        };
        await notificationRepository.CreateAsync(notification, ct);
    }

    private static NotificationResponse Map(Notification notification)
        => new(notification.Id, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt);
}
