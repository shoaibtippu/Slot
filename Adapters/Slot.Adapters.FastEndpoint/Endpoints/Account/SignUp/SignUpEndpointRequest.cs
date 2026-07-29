namespace Slot.Adapters.FastEndpoint.Endpoints.Account.SignUp;

public class SignUpEndpointRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}