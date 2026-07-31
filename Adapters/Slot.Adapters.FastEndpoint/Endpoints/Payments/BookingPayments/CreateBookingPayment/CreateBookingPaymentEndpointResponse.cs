namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.BookingPayments.CreateBookingPayment;

public class CreateBookingPaymentEndpointResponse
{
    public bool Success { get; set; }
    public BookingPaymentRecordResponse? Payment { get; set; }
    public string? Error { get; set; }
}
