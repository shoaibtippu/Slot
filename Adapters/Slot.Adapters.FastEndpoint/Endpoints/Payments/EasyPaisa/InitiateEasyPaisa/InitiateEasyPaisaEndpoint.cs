using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.EasyPaisa.InitiateEasyPaisa;

[Authorize]
public class InitiateEasyPaisa(IPaymentService paymentService) : Endpoint<InitiateEasyPaisaEndpointRequest, InitiateEasyPaisaEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/payments/easypaisa/initiate");
        Summary(s =>
        {
            s.Summary = "Initiate EasyPaisa payment.";
            s.Response<InitiateEasyPaisaEndpointResponse>(200, "Payment initiation created successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(InitiateEasyPaisaEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new InitiateEasyPaisaEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await paymentService.InitiateEasyPaisaAsync(identityId, new PaymentInitiationRequest(req.BookingId), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new InitiateEasyPaisaEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new InitiateEasyPaisaEndpointResponse { Success = true, Payment = result.Value, Error = null }, ct);
    }
}
