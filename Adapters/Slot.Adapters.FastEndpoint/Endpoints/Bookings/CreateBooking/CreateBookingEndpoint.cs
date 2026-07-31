using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CreateBooking;

[Authorize]
public class CreateBooking(IBookingService bookingService) : Endpoint<CreateBookingEndpointRequest, CreateBookingEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/bookings");
        Summary(s =>
        {
            s.Summary = "Create a booking.";
            s.Response<CreateBookingEndpointResponse>(200, "Booking created successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CreateBookingEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CreateBookingEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await bookingService.CreateAsync(identityId, new CreateBookingRequest(req.GroundId, req.BookingDate, req.StartTime, req.EndTime, req.Notes), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CreateBookingEndpointResponse { Success = false, Booking = null, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CreateBookingEndpointResponse { Success = true, Booking = result.Value, Error = null }, ct);
    }
}
