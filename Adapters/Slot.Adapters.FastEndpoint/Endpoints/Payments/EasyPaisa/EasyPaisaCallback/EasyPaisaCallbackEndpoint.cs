namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.EasyPaisa.EasyPaisaCallback;

public class EasyPaisaCallback(IPaymentService paymentService) : Endpoint<EasyPaisaCallbackEndpointRequest, EasyPaisaCallbackEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/payments/easypaisa/callback");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "EasyPaisa payment callback webhook.";
            s.Response<EasyPaisaCallbackEndpointResponse>(200, "Callback processed successfully.");
        });
    }

    public override async Task HandleAsync(EasyPaisaCallbackEndpointRequest req, CancellationToken ct)
    {
        var result = await paymentService.EasyPaisaCallbackAsync(new EasyPaisaCallbackRequest(req.BookingId, req.TransactionReference, req.Amount, req.Success, req.RawPayload), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new EasyPaisaCallbackEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new EasyPaisaCallbackEndpointResponse { Success = true, Payment = result.Value, Error = null }, ct);
    }
}
