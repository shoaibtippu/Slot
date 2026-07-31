using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.BookingPayments.GetBookingPayments;

[Authorize]
public class GetBookingPayments(IPaymentService paymentService) : EndpointWithoutRequest<GetBookingPaymentsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/bookings/{id}/payments");
        Summary(s =>
        {
            s.Summary = "List all payments for a booking.";
            s.Response<GetBookingPaymentsEndpointResponse>(200, "Payments retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetBookingPaymentsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var bookingId))
        {
            await Send.ResponseAsync(new GetBookingPaymentsEndpointResponse { Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await paymentService.GetBookingPaymentsAsync(identityId, bookingId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetBookingPaymentsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetBookingPaymentsEndpointResponse { Payments = result.Value!, Error = null }, ct);
    }
}
