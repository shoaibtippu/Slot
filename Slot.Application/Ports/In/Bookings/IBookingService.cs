namespace Slot.Application.Ports.In.Bookings;

public interface IBookingService
{
    Task<Result<BookingDetailResponse>> CreateAsync(string userIdentityId, CreateBookingRequest request, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingListItemResponse>>> GetMyBookingsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<BookingListItemResponse>>> GetGroundOwnerBookingsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result<BookingDetailResponse>> GetByIdAsync(string userIdentityId, Guid id, CancellationToken ct = default);
    Task<Result<BookingDetailResponse>> UpdateStatusAsync(string userIdentityId, Guid id, UpdateBookingStatusRequest request, CancellationToken ct = default);
    Task<Result<BookingDetailResponse>> CancelAsync(string userIdentityId, Guid id, CancellationToken ct = default);
    Task<Result<BookingDetailResponse>> CompleteAsync(string userIdentityId, Guid id, CancellationToken ct = default);
}

public record CreateBookingRequest(Guid GroundId, DateOnly BookingDate, TimeSpan StartTime, TimeSpan EndTime, string? Notes);
public record UpdateBookingStatusRequest(BookingStatus Status);

public record BookingListItemResponse(
    Guid Id,
    Guid GroundId,
    string? GroundName,
    DateOnly BookingDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    BookingStatus Status,
    decimal TotalAmount,
    decimal AdvanceAmount,
    decimal RemainingAmount,
    string? UserEmail = null);

public record BookingDetailResponse(
    Guid Id,
    Guid GroundId,
    string? GroundName,
    Guid UserId,
    string? UserEmail,
    DateOnly BookingDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal PricePerHour,
    decimal TotalAmount,
    decimal AdvanceAmount,
    decimal RemainingAmount,
    BookingStatus Status,
    string? Notes,
    IReadOnlyList<BookingPaymentResponse> Payments);

public record BookingPaymentResponse(Guid Id, decimal Amount, PaymentMethod Method, PaymentStatus Status, string? TransactionReference, DateTime? PaidAt);
