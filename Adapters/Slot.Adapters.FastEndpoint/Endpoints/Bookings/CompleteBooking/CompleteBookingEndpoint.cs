using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CompleteBooking;

[Authorize]
public class CompleteBooking(IBookingService bookingService) : EndpointWithoutRequest<CompleteBookingEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/bookings/{id}/complete");
        Summary(s =>
        {
            s.Summary = "Mark a booking complete.";
            s.Response<CompleteBookingEndpointResponse>(200, "Booking completed successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CompleteBookingEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new CompleteBookingEndpointResponse { Success = false, Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await bookingService.CompleteAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CompleteBookingEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CompleteBookingEndpointResponse { Success = true, Booking = result.Value, Error = null }, ct);
    }
}
