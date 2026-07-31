namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.JazzCash.JazzCashCallback;

public class JazzCashCallback(IPaymentService paymentService) : Endpoint<JazzCashCallbackEndpointRequest, JazzCashCallbackEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/payments/jazzcash/callback");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "JazzCash payment callback webhook.";
            s.Response<JazzCashCallbackEndpointResponse>(200, "Callback processed successfully.");
        });
    }

    public override async Task HandleAsync(JazzCashCallbackEndpointRequest req, CancellationToken ct)
    {
        var result = await paymentService.JazzCashCallbackAsync(new JazzCashCallbackRequest(req.BookingId, req.TransactionReference, req.Amount, req.Success, req.RawPayload), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new JazzCashCallbackEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new JazzCashCallbackEndpointResponse { Success = true, Payment = result.Value, Error = null }, ct);
    }
}
