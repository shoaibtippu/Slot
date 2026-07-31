using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetGroundById;

[Authorize(Roles = "Admin")]
public class GetGroundById(IAdminService adminService) : EndpointWithoutRequest<GetGroundByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/grounds/{id}");
        Summary(s =>
        {
            s.Summary = "Ground detail (admin).";
            s.Response<GetGroundByIdEndpointResponse>(200, "Ground retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetGroundByIdEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await adminService.GetGroundByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundByIdEndpointResponse { Ground = result.Value!, Error = null }, ct);
    }
}
