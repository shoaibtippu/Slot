using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetBookingById;

[Authorize(Roles = "Admin")]
public class GetBookingById(IAdminService adminService) : EndpointWithoutRequest<GetBookingByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/bookings/{id}");
        Summary(s =>
        {
            s.Summary = "Booking detail (admin).";
            s.Response<GetBookingByIdEndpointResponse>(200, "Booking retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetBookingByIdEndpointResponse { Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await adminService.GetBookingByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetBookingByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetBookingByIdEndpointResponse { Booking = result.Value!, Error = null }, ct);
    }
}
