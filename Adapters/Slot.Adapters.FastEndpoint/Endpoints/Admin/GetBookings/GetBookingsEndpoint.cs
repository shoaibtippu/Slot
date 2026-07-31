using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetBookings;

[Authorize(Roles = "Admin")]
public class GetBookings(IAdminService adminService) : EndpointWithoutRequest<GetBookingsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/bookings");
        Summary(s =>
        {
            s.Summary = "Platform-wide bookings overview.";
            s.Response<GetBookingsEndpointResponse>(200, "Bookings retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await adminService.GetBookingsAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetBookingsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetBookingsEndpointResponse { Bookings = result.Value!, Error = null }, ct);
    }
}
