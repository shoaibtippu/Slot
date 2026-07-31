using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.UpdateUserRoles;

[Authorize(Roles = "Admin")]
public class UpdateUserRoles(IAdminService adminService) : Endpoint<UpdateUserRolesEndpointRequest, UpdateUserRolesEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/admin/users/{id}/roles");
        Summary(s =>
        {
            s.Summary = "Assign or remove role from user.";
            s.Response<UpdateUserRolesEndpointResponse>(200, "Roles updated successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(UpdateUserRolesEndpointRequest req, CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UpdateUserRolesEndpointResponse { Success = false, Error = "Invalid user id." }, 400, ct);
            return;
        }

        var result = await adminService.UpdateUserRolesAsync(id, new UpdateUserRolesRequest(req.Roles), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateUserRolesEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateUserRolesEndpointResponse { Success = true, Error = null }, ct);
    }
}
