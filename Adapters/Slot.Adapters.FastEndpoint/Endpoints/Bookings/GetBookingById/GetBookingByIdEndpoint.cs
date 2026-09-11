using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetBookingById;

[Authorize]
public class GetBookingById(IBookingService bookingService) : EndpointWithoutRequest<GetBookingByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/bookings/{id:guid}");
        Summary(s =>
        {
            s.Summary = "Get booking detail with payments.";
            s.Response<GetBookingByIdEndpointResponse>(200, "Booking retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetBookingByIdEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetBookingByIdEndpointResponse { Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await bookingService.GetByIdAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetBookingByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetBookingByIdEndpointResponse { Booking = result.Value, Error = null }, ct);
    }
}
