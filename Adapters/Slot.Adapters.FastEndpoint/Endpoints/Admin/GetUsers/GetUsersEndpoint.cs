using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetUsers;

[Authorize(Roles = "Admin")]
public class GetUsers(IAdminService adminService) : EndpointWithoutRequest<GetUsersEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/users");
        Summary(s =>
        {
            s.Summary = "List all users with roles.";
            s.Response<GetUsersEndpointResponse>(200, "Users retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await adminService.GetUsersAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetUsersEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetUsersEndpointResponse { Users = result.Value!, Error = null }, ct);
    }
}
