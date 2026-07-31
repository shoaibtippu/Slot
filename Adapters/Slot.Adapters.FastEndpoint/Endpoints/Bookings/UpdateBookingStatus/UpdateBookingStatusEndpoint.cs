using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.UpdateBookingStatus;

[Authorize]
public class UpdateBookingStatus(IBookingService bookingService) : Endpoint<UpdateBookingStatusEndpointRequest, UpdateBookingStatusEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/bookings/{id}/status");
        Summary(s =>
        {
            s.Summary = "Approve or reject a booking.";
            s.Response<UpdateBookingStatusEndpointResponse>(200, "Booking status updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(UpdateBookingStatusEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateBookingStatusEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UpdateBookingStatusEndpointResponse { Success = false, Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await bookingService.UpdateStatusAsync(identityId, id, new UpdateBookingStatusRequest(req.Status), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateBookingStatusEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateBookingStatusEndpointResponse { Success = true, Booking = result.Value, Error = null }, ct);
    }
}
