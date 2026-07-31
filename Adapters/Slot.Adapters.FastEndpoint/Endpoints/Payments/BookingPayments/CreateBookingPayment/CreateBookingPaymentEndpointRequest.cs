namespace Slot.Adapters.FastEndpoint.Endpoints.Payments.BookingPayments.CreateBookingPayment;

public class CreateBookingPaymentEndpointRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionReference { get; set; }
}
