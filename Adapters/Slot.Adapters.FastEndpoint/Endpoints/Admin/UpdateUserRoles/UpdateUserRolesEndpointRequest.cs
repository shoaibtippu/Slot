namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.UpdateUserRoles;

public class UpdateUserRolesEndpointRequest
{
    public IReadOnlyList<string> Roles { get; set; } = [];
}
