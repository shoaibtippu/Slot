namespace Slot.Application.Services;

public class AdminService(
    IAdminRepository adminRepository,
    IUserRepository userRepository,
    IGroundRepository groundRepository,
    IBookingRepository bookingRepository,
    IPaymentRepository paymentRepository,
    UserManager<IdentityUser> userManager) : IAdminService
{
    public async Task<Result<IReadOnlyList<AdminUserListItemResponse>>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await adminRepository.GetUsersAsync(ct);
        return Result.Success<IReadOnlyList<AdminUserListItemResponse>>(users.Select(MapUser).ToList());
    }

    public async Task<Result<AdminUserDetailResponse>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await adminRepository.GetUserByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure<AdminUserDetailResponse>(Error.NotFound("User not found."));

        return Result.Success(MapUserDetail(user.Value));
    }

    public async Task<Result> UpdateUserRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken ct = default)
    {
        var user = await adminRepository.GetUserByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User not found."));

        var validRoles = new[] { "Admin", "User" };
        var roles = request.Roles.Where(r => validRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        await adminRepository.UpdateUserRolesAsync(user.Value.Identity.Id, roles, ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await adminRepository.GetUserByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User not found."));

        await adminRepository.DeactivateUserAsync(user.Value.Identity.Id, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AdminGroundListItemResponse>>> GetGroundsAsync(CancellationToken ct = default)
    {
        var grounds = await adminRepository.GetGroundsAsync(ct);
        return Result.Success<IReadOnlyList<AdminGroundListItemResponse>>(grounds.Select(MapGround).ToList());
    }

    public async Task<Result<AdminGroundDetailResponse>> GetGroundByIdAsync(Guid groundId, CancellationToken ct = default)
    {
        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<AdminGroundDetailResponse>(Error.NotFound("Ground not found."));

        return Result.Success(MapGroundDetail(ground));
    }

    public async Task<Result> ForceDeleteGroundAsync(Guid groundId, CancellationToken ct = default)
    {
        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure(Error.NotFound("Ground not found."));

        await adminRepository.DeleteGroundAsync(ground, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AdminBookingListItemResponse>>> GetBookingsAsync(CancellationToken ct = default)
    {
        var bookings = await adminRepository.GetBookingsAsync(ct);
        return Result.Success<IReadOnlyList<AdminBookingListItemResponse>>(bookings.Select(MapBooking).ToList());
    }

    public async Task<Result<AdminBookingDetailResponse>> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await bookingRepository.FindByIdWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure<AdminBookingDetailResponse>(Error.NotFound("Booking not found."));

        return Result.Success(MapBookingDetail(booking));
    }

    public async Task<Result<IReadOnlyList<AdminTransactionListItemResponse>>> GetTransactionsAsync(CancellationToken ct = default)
    {
        var payments = await adminRepository.GetPaymentsAsync(ct);
        return Result.Success<IReadOnlyList<AdminTransactionListItemResponse>>(payments.Select(MapTransaction).ToList());
    }

    public async Task<Result<AdminTransactionDetailResponse>> GetTransactionByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await paymentRepository.FindByIdAsync(paymentId, ct);
        if (payment is null)
            return Result.Failure<AdminTransactionDetailResponse>(Error.NotFound("Transaction not found."));

        var loaded = await paymentRepository.GetByBookingIdAsync(payment.BookingId, ct);
        var booking = loaded.FirstOrDefault(p => p.Id == payment.Id) ?? payment;
        return Result.Success(MapTransactionDetail(booking));
    }

    public async Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken ct = default)
    {
        var totalUsers = await adminRepository.GetUserCountAsync(ct);
        var totalGrounds = await adminRepository.GetGroundCountAsync(ct);
        var totalBookings = await adminRepository.GetBookingCountAsync(ct);
        var totalRevenue = await adminRepository.GetTotalRevenueAsync(ct);
        var pendingRevenue = await adminRepository.GetPendingRevenueAsync(ct);
        var completedRevenue = await adminRepository.GetCompletedRevenueAsync(ct);

        return Result.Success(new AdminDashboardResponse(totalUsers, totalGrounds, totalBookings, totalRevenue, pendingRevenue, completedRevenue));
    }

    private static AdminUserListItemResponse MapUser((User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive) user)
        => new(user.Profile.Id, user.Identity.Email ?? string.Empty, null, user.Identity.PhoneNumber, user.Profile.ImageUrl, user.Roles, user.IsActive);

    private static AdminUserDetailResponse MapUserDetail((User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive) user)
        => new(user.Profile.Id, user.Identity.Email ?? string.Empty, null, user.Identity.PhoneNumber, user.Profile.ImageUrl, user.Roles, user.IsActive, user.Profile.CreatedAt, user.Profile.ModifiedAt);

    private static AdminGroundListItemResponse MapGround(Ground ground)
        => new(ground.Id, ground.Name, ground.Address, ground.HourlyRate, ground.AverageRating, ground.OwnerId, ground.Owner.UserIdentity.Email, true, ground.CreatedAt);

    private static AdminGroundDetailResponse MapGroundDetail(Ground ground)
        => new(ground.Id, ground.Name, ground.Description, ground.Address, ground.Latitude, ground.Longitude, ground.PhoneNumber, ground.AlternatePhoneNumber, ground.HourlyRate, ground.AdvancePercentage, ground.AverageRating, ground.TotalReviews, ground.OwnerId, ground.Owner.UserIdentity.Email, true, ground.CreatedAt, ground.ModifiedAt);

    private static AdminBookingListItemResponse MapBooking(Booking booking)
        => new(booking.Id, booking.GroundId, booking.Ground.Name, booking.UserId, booking.User.UserIdentity.Email, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Status, booking.TotalAmount, booking.AdvanceAmount, booking.RemainingAmount, booking.CreatedAt);

    private static AdminBookingDetailResponse MapBookingDetail(Booking booking)
        => new(booking.Id, booking.GroundId, booking.Ground.Name, booking.UserId, booking.User.UserIdentity.Email, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Status, booking.PricePerHour, booking.TotalAmount, booking.AdvanceAmount, booking.RemainingAmount, booking.Notes, booking.Payments.Select(p => new BookingPaymentResponse(p.Id, p.Amount, p.Method, p.Status, p.TransactionReference, p.PaidAt)).ToList(), booking.CreatedAt, booking.ModifiedAt);

    private static AdminTransactionListItemResponse MapTransaction(Payment payment)
        => new(payment.Id, payment.BookingId, payment.Booking.Ground.Name, payment.Booking.UserId, payment.Booking.User.UserIdentity.Email, payment.Amount, payment.Method, payment.Status, payment.TransactionReference, payment.PaidAt, payment.CreatedAt);

    private static AdminTransactionDetailResponse MapTransactionDetail(Payment payment)
        => new(payment.Id, payment.BookingId, payment.Booking.Ground.Name, payment.Booking.UserId, payment.Booking.User.UserIdentity.Email, payment.Amount, payment.Method, payment.Status, payment.TransactionReference, payment.PaidAt, payment.CreatedAt, payment.ModifiedAt);
}
