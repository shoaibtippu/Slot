using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetDashboard;

[Authorize(Roles = "Admin")]
public class GetDashboard(IAdminService adminService) : EndpointWithoutRequest<GetDashboardEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/dashboard");
        Summary(s =>
        {
            s.Summary = "Stats summary (users, grounds, revenue).";
            s.Response<GetDashboardEndpointResponse>(200, "Dashboard retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await adminService.GetDashboardAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetDashboardEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetDashboardEndpointResponse { Dashboard = result.Value!, Error = null }, ct);
    }
}
