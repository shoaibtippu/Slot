using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.DeactivateUser;

[Authorize(Roles = "Admin")]
public class DeactivateUser(IAdminService adminService) : EndpointWithoutRequest<DeactivateUserEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/admin/users/{id}");
        Summary(s =>
        {
            s.Summary = "Deactivate / soft-delete user.";
            s.Response<DeactivateUserEndpointResponse>(200, "User deactivated successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new DeactivateUserEndpointResponse { Success = false, Error = "Invalid user id." }, 400, ct);
            return;
        }

        var result = await adminService.DeactivateUserAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeactivateUserEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeactivateUserEndpointResponse { Success = true, Error = null }, ct);
    }
}
