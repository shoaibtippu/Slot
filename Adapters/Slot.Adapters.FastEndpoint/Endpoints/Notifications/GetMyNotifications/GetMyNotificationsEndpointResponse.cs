namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.GetMyNotifications;

public class GetMyNotificationsEndpointResponse
{
    public IReadOnlyList<NotificationResponse> Notifications { get; set; } = [];
    public string? Error { get; set; }
}
