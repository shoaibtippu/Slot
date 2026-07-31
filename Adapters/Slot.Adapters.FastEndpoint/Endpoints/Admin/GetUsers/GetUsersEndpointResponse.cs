namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetUsers;

public class GetUsersEndpointResponse
{
    public IReadOnlyList<AdminUserListItemResponse> Users { get; set; } = [];
    public string? Error { get; set; }
}
