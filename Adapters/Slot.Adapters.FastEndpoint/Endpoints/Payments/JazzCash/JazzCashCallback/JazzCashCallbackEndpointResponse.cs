namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.JazzCash.JazzCashCallback;

public class JazzCashCallbackEndpointResponse
{
    public bool Success { get; set; }
    public PaymentCallbackResponse? Payment { get; set; }
    public string? Error { get; set; }
}
