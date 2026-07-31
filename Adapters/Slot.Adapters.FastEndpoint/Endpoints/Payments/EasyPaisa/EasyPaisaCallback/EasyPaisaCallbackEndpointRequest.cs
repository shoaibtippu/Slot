namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.EasyPaisa.EasyPaisaCallback;

public class EasyPaisaCallbackEndpointRequest
{
    public Guid BookingId { get; set; }
    public string TransactionReference { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool Success { get; set; }
    public string? RawPayload { get; set; }
}
