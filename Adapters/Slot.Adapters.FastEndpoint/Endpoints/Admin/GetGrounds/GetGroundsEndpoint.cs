using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetGrounds;

[Authorize(Roles = "Admin")]
public class GetGrounds(IAdminService adminService) : EndpointWithoutRequest<GetGroundsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/grounds");
        Summary(s =>
        {
            s.Summary = "All grounds across platform.";
            s.Response<GetGroundsEndpointResponse>(200, "Grounds retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await adminService.GetGroundsAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundsEndpointResponse { Grounds = result.Value!, Error = null }, ct);
    }
}
