namespace Slot.Application.Ports.In.Notifications;

public record RegisterPushTokenRequest(string Token, string? Platform);
public record UnregisterPushTokenRequest(string Token);
