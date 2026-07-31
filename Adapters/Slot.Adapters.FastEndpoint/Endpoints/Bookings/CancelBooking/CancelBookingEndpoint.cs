using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CancelBooking;

[Authorize]
public class CancelBooking(IBookingService bookingService) : EndpointWithoutRequest<CancelBookingEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/bookings/{id}/cancel");
        Summary(s =>
        {
            s.Summary = "Cancel a booking.";
            s.Response<CancelBookingEndpointResponse>(200, "Booking cancelled successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CancelBookingEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new CancelBookingEndpointResponse { Success = false, Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await bookingService.CancelAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CancelBookingEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CancelBookingEndpointResponse { Success = true, Booking = result.Value, Error = null }, ct);
    }
}
