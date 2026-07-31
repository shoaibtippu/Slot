namespace Slot.Adapters.FastEndpoint.Endpoints.Account.ResetPassword;

public class ResetPasswordEndpointRequest
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}