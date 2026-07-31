namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.EasyPaisa.InitiateEasyPaisa;

public class InitiateEasyPaisaEndpointResponse
{
    public bool Success { get; set; }
    public PaymentInitiationResponse? Payment { get; set; }
    public string? Error { get; set; }
}
