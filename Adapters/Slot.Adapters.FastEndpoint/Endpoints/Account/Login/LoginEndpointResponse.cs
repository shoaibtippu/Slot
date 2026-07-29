namespace Slot.Adapters.FastEndpoint.Endpoints.Account.Login;

public record LoginEndpointResponse(string? AccessToken, string? Email, Guid? UserId, string? Error);