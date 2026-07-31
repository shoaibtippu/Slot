using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetUserById;

[Authorize(Roles = "Admin")]
public class GetUserById(IAdminService adminService) : EndpointWithoutRequest<GetUserByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/users/{id}");
        Summary(s =>
        {
            s.Summary = "User detail (admin).";
            s.Response<GetUserByIdEndpointResponse>(200, "User retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetUserByIdEndpointResponse { Error = "Invalid user id." }, 400, ct);
            return;
        }

        var result = await adminService.GetUserByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetUserByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetUserByIdEndpointResponse { User = result.Value!, Error = null }, ct);
    }
}
