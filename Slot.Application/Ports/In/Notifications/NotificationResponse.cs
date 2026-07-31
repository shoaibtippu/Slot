namespace Slot.Application.Ports.In.Notifications;

public record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTime? CreatedAt);
