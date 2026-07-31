namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.EasyPaisa.EasyPaisaCallback;

public class EasyPaisaCallbackEndpointResponse
{
    public bool Success { get; set; }
    public PaymentCallbackResponse? Payment { get; set; }
    public string? Error { get; set; }
}
