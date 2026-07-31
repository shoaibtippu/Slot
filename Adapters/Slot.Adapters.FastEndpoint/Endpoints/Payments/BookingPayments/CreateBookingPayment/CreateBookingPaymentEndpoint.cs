using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.BookingPayments.CreateBookingPayment;

[Authorize]
public class CreateBookingPayment( IPaymentService paymentService) : Endpoint<CreateBookingPaymentEndpointRequest, CreateBookingPaymentEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/bookings/{id}/payments");
        Summary(s =>
        {
            s.Summary = "Record a payment against a booking.";
            s.Response<CreateBookingPaymentEndpointResponse>(200, "Payment recorded successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CreateBookingPaymentEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CreateBookingPaymentEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var bookingId))
        {
            await Send.ResponseAsync(new CreateBookingPaymentEndpointResponse { Success = false, Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await paymentService.RecordBookingPaymentAsync(identityId, bookingId, new RecordBookingPaymentRequest(req.Amount, req.Method, req.TransactionReference), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CreateBookingPaymentEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CreateBookingPaymentEndpointResponse { Success = true, Payment = result.Value, Error = null }, ct);
    }
}
