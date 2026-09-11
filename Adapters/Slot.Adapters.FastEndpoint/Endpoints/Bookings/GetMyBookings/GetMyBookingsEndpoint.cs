using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetMyBookings;

[Authorize]
public class GetMyBookings(IBookingService bookingService) : EndpointWithoutRequest<GetMyBookingsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/bookings/my");
        Summary(s =>
        {
            s.Summary = "Get my bookings.";
            s.Response<GetMyBookingsEndpointResponse>(200, "Bookings retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetMyBookingsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await bookingService.GetMyBookingsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetMyBookingsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetMyBookingsEndpointResponse { Bookings = result.Value!, Error = null }, ct);
    }
}
