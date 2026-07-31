namespace Slot.Application.Ports.In.Admin;

public interface IAdminService
{
    Task<Result<IReadOnlyList<AdminUserListItemResponse>>> GetUsersAsync(CancellationToken ct = default);
    Task<Result<AdminUserDetailResponse>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result> UpdateUserRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken ct = default);
    Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AdminGroundListItemResponse>>> GetGroundsAsync(CancellationToken ct = default);
    Task<Result> ForceDeleteGroundAsync(Guid groundId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AdminBookingListItemResponse>>> GetBookingsAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<AdminTransactionListItemResponse>>> GetTransactionsAsync(CancellationToken ct = default);
    Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken ct = default);
}

public record UpdateUserRolesRequest(IReadOnlyList<string> Roles);

public record AdminUserListItemResponse(
    Guid UserId,
    string Email,
    string? FullName,
    string? PhoneNumber,
    string? ImageUrl,
    IReadOnlyList<string> Roles,
    bool IsActive);

public record AdminUserDetailResponse(
    Guid UserId,
    string Email,
    string? FullName,
    string? PhoneNumber,
    string? ImageUrl,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime? CreatedAt,
    DateTime? ModifiedAt);

public record AdminGroundListItemResponse(
    Guid GroundId,
    string? Name,
    string? Address,
    decimal HourlyRate,
    decimal AverageRating,
    Guid OwnerId,
    string? OwnerEmail,
    bool IsActive,
    DateTime? CreatedAt);

public record AdminBookingListItemResponse(
    Guid BookingId,
    Guid GroundId,
    string? GroundName,
    Guid UserId,
    string? UserEmail,
    DateOnly BookingDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    BookingStatus Status,
    decimal TotalAmount,
    decimal AdvanceAmount,
    decimal RemainingAmount,
    DateTime? CreatedAt);

public record AdminTransactionListItemResponse(
    Guid PaymentId,
    Guid BookingId,
    string? BookingGroundName,
    Guid UserId,
    string? UserEmail,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    string? TransactionReference,
    DateTime? PaidAt,
    DateTime? CreatedAt);

public record AdminDashboardResponse(
    int TotalUsers,
    int TotalGrounds,
    int TotalBookings,
    decimal TotalRevenue,
    decimal PendingRevenue,
    decimal CompletedRevenue);
