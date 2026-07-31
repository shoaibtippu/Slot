namespace Slot.Adapters.FastEndpoint.Endpoints.Account.ChangePassword;

public class ChangePasswordEndpointRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}