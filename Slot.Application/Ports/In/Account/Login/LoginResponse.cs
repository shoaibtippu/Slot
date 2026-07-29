namespace Slot.Application.Ports.In.Account.Login;

public record LoginResponse(string AccessToken, string Email, Guid UserId);