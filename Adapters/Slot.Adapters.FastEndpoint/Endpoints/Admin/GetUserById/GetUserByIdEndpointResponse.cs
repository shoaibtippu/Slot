namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetUserById;

public class GetUserByIdEndpointResponse
{
    public AdminUserDetailResponse? User { get; set; }
    public string? Error { get; set; }
}
