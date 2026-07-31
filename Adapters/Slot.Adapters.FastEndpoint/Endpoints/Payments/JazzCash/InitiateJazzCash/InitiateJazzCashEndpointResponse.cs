namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.JazzCash.InitiateJazzCash;

public class InitiateJazzCashEndpointResponse
{
    public bool Success { get; set; }
    public PaymentInitiationResponse? Payment { get; set; }
    public string? Error { get; set; }
}
