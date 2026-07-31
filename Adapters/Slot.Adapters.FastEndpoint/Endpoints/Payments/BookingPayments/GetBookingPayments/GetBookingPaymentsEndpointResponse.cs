namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.BookingPayments.GetBookingPayments;

public class GetBookingPaymentsEndpointResponse
{
    public IReadOnlyList<BookingPaymentRecordResponse> Payments { get; set; } = [];
    public string? Error { get; set; }
}
