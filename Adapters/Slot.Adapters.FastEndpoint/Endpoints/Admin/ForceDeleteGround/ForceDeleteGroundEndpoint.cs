using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.ForceDeleteGround;

[Authorize(Roles = "Admin")]
public class ForceDeleteGround(IAdminService adminService) : EndpointWithoutRequest<ForceDeleteGroundEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/admin/grounds/{id}");
        Summary(s =>
        {
            s.Summary = "Force-delete any ground.";
            s.Response<ForceDeleteGroundEndpointResponse>(200, "Ground deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new ForceDeleteGroundEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await adminService.ForceDeleteGroundAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new ForceDeleteGroundEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new ForceDeleteGroundEndpointResponse { Success = true, Error = null }, ct);
    }
}
