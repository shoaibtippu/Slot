namespace Slot.Adapters.FastEndpoint.Endpoints.Account.GetProfile;

public class GetProfileEndpointResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public IList<string> Roles { get; set; } = [];
}