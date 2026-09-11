using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetGroundOwnerBookings;

[Authorize]
public class GetGroundOwnerBookings(IBookingService bookingService) : EndpointWithoutRequest<GetGroundOwnerBookingsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/bookings/owner");
        Summary(s =>
        {
            s.Summary = "Get bookings for grounds I own.";
            s.Response<GetGroundOwnerBookingsEndpointResponse>(200, "Bookings retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetGroundOwnerBookingsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await bookingService.GetGroundOwnerBookingsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundOwnerBookingsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundOwnerBookingsEndpointResponse { Bookings = result.Value!, Error = null }, ct);
    }
}
