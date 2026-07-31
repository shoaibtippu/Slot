using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.JazzCash.InitiateJazzCash;

[Authorize]
public class InitiateJazzCash(IPaymentService paymentService) : Endpoint<InitiateJazzCashEndpointRequest, InitiateJazzCashEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/payments/jazzcash/initiate");
        Summary(s =>
        {
            s.Summary = "Initiate JazzCash payment.";
            s.Response<InitiateJazzCashEndpointResponse>(200, "Payment initiation created successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(InitiateJazzCashEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new InitiateJazzCashEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await paymentService.InitiateJazzCashAsync(identityId, new PaymentInitiationRequest(req.BookingId), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new InitiateJazzCashEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new InitiateJazzCashEndpointResponse { Success = true, Payment = result.Value, Error = null }, ct);
    }
}
