namespace Slot.Application.Ports.In.Payments;

public interface IPaymentService
{
    Task<Result<BookingPaymentRecordResponse>> RecordBookingPaymentAsync(string userIdentityId, Guid bookingId, RecordBookingPaymentRequest request, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingPaymentRecordResponse>>> GetBookingPaymentsAsync(string userIdentityId, Guid bookingId, CancellationToken ct = default);
    Task<Result<PaymentInitiationResponse>> InitiateJazzCashAsync(string userIdentityId, PaymentInitiationRequest request, CancellationToken ct = default);
    Task<Result<PaymentCallbackResponse>> JazzCashCallbackAsync(JazzCashCallbackRequest request, CancellationToken ct = default);
    Task<Result<PaymentInitiationResponse>> InitiateEasyPaisaAsync(string userIdentityId, PaymentInitiationRequest request, CancellationToken ct = default);
    Task<Result<PaymentCallbackResponse>> EasyPaisaCallbackAsync(EasyPaisaCallbackRequest request, CancellationToken ct = default);
}

public record RecordBookingPaymentRequest(decimal Amount, PaymentMethod Method, string? TransactionReference);
public record BookingPaymentRecordResponse(Guid Id, Guid BookingId, decimal Amount, PaymentMethod Method, PaymentStatus Status, string? TransactionReference, DateTime? PaidAt);
public record PaymentInitiationRequest(Guid BookingId);
public record PaymentInitiationResponse(Guid BookingId, PaymentMethod Method, string TransactionReference, string PaymentUrl, decimal Amount, string Message);
public record JazzCashCallbackRequest(Guid BookingId, string TransactionReference, decimal Amount, bool Success, string? RawPayload);
public record EasyPaisaCallbackRequest(Guid BookingId, string TransactionReference, decimal Amount, bool Success, string? RawPayload);
public record PaymentCallbackResponse(Guid BookingId, Guid PaymentId, PaymentStatus Status, string TransactionReference, string Message);
