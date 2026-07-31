namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.RegisterPushToken;

public class RegisterPushTokenEndpointRequest
{
    public string Token { get; set; } = null!;
    public string? Platform { get; set; }
}
