namespace Slot.Application.Services;

public class BookingService(IBookingRepository bookingRepository, IGroundRepository groundRepository, IUserRepository userRepository) : IBookingService
{
    public async Task<Result<BookingDetailResponse>> CreateAsync(string userIdentityId, CreateBookingRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(request.GroundId, ct);
        if (ground is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Ground not found."));

        if (request.EndTime <= request.StartTime)
            return Result.Failure<BookingDetailResponse>(Error.Validation("End time must be greater than start time."));

        var schedules = await groundRepository.GetSchedulesAsync(request.GroundId, ct);
        var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == request.BookingDate.DayOfWeek && !s.IsClosed);
        if (schedule is null)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Ground is closed on the selected day."));

        if (request.StartTime < schedule.OpeningTime || request.EndTime > schedule.ClosingTime)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Requested time is outside opening hours."));

        var blocks = await groundRepository.GetAvailabilityBlocksAsync(request.GroundId, request.BookingDate, ct);
        if (blocks.Any(b => Overlaps(request.StartTime, request.EndTime, b.StartTime, b.EndTime)))
            return Result.Failure<BookingDetailResponse>(Error.Conflict("Selected slot is blocked."));

        var bookings = await bookingRepository.GetByGroundAndDateAsync(request.GroundId, request.BookingDate, ct);
        if (bookings.Any(b => Overlaps(request.StartTime, request.EndTime, b.StartTime, b.EndTime) && b.Status is BookingStatus.Pending or BookingStatus.Approved))
            return Result.Failure<BookingDetailResponse>(Error.Conflict("Selected slot is already booked."));

        var durationHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
        var totalAmount = Math.Round(durationHours * ground.HourlyRate, 2, MidpointRounding.AwayFromZero);
        var advanceAmount = Math.Round(totalAmount * ground.AdvancePercentage / 100m, 2, MidpointRounding.AwayFromZero);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            GroundId = ground.Id,
            UserId = profile.Id,
            BookingDate = request.BookingDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            PricePerHour = ground.HourlyRate,
            TotalAmount = totalAmount,
            AdvanceAmount = advanceAmount,
            RemainingAmount = totalAmount - advanceAmount,
            Status = BookingStatus.Pending,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        await bookingRepository.CreateAsync(booking, ct);
        var created = await bookingRepository.FindByIdWithDetailsAsync(booking.Id, ct);
        return created is null ? Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found.")) : Result.Success(MapDetail(created));
    }

    public async Task<Result<IReadOnlyList<BookingListItemResponse>>> GetMyBookingsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<BookingListItemResponse>>(Error.NotFound("User not found."));

        var bookings = await bookingRepository.GetMyBookingsAsync(profile.Id, ct);
        return Result.Success<IReadOnlyList<BookingListItemResponse>>(bookings.Select(b => MapListItem(b)).ToList());
    }

    public async Task<Result<IReadOnlyList<BookingListItemResponse>>> GetGroundOwnerBookingsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<BookingListItemResponse>>(Error.NotFound("User not found."));

        var bookings = await bookingRepository.GetGroundOwnerBookingsAsync(profile.Id, ct);
        return Result.Success<IReadOnlyList<BookingListItemResponse>>(bookings.Select(b => MapListItem(b, includeUserEmail: true)).ToList());
    }

    public async Task<Result<BookingDetailResponse>> GetByIdAsync(string userIdentityId, Guid id, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(id, ct);
        if (booking is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id && booking.Ground.OwnerId != profile.Id)
            return Result.Failure<BookingDetailResponse>(Error.UnAuthorized("You are not allowed to view this booking."));

        return Result.Success(MapDetail(booking));
    }

    public async Task<Result<BookingDetailResponse>> UpdateStatusAsync(string userIdentityId, Guid id, UpdateBookingStatusRequest request, CancellationToken ct = default)
    {
        if (request.Status is not BookingStatus.Approved and not BookingStatus.Rejected)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Status must be Approved or Rejected."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(id, ct);
        if (booking is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found."));

        if (booking.Ground.OwnerId != profile.Id)
            return Result.Failure<BookingDetailResponse>(Error.UnAuthorized("You are not allowed to modify this booking."));

        if (booking.Status != BookingStatus.Pending)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Only pending bookings can be updated."));

        booking.Status = request.Status;
        await bookingRepository.UpdateAsync(booking, ct);

        var updated = await bookingRepository.FindByIdWithDetailsAsync(booking.Id, ct);
        return updated is null ? Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found.")) : Result.Success(MapDetail(updated));
    }

    public async Task<Result<BookingDetailResponse>> CancelAsync(string userIdentityId, Guid id, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(id, ct);
        if (booking is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id)
            return Result.Failure<BookingDetailResponse>(Error.UnAuthorized("You are not allowed to cancel this booking."));

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed or BookingStatus.Rejected)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Booking cannot be cancelled."));

        booking.Status = BookingStatus.Cancelled;
        await bookingRepository.UpdateAsync(booking, ct);

        var updated = await bookingRepository.FindByIdWithDetailsAsync(booking.Id, ct);
        return updated is null ? Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found.")) : Result.Success(MapDetail(updated));
    }

    public async Task<Result<BookingDetailResponse>> CompleteAsync(string userIdentityId, Guid id, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(id, ct);
        if (booking is null)
            return Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found."));

        if (booking.Ground.OwnerId != profile.Id)
            return Result.Failure<BookingDetailResponse>(Error.UnAuthorized("You are not allowed to complete this booking."));

        if (booking.Status != BookingStatus.Approved)
            return Result.Failure<BookingDetailResponse>(Error.Validation("Only approved bookings can be completed."));

        booking.Status = BookingStatus.Completed;
        await bookingRepository.UpdateAsync(booking, ct);

        var updated = await bookingRepository.FindByIdWithDetailsAsync(booking.Id, ct);
        return updated is null ? Result.Failure<BookingDetailResponse>(Error.NotFound("Booking not found.")) : Result.Success(MapDetail(updated));
    }

    private static bool Overlaps(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        => start1 < end2 && start2 < end1;

    private static BookingListItemResponse MapListItem(Booking booking, bool includeUserEmail = false)
        => new(booking.Id, booking.GroundId, booking.Ground.Name, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Status, booking.TotalAmount, booking.AdvanceAmount, booking.RemainingAmount, includeUserEmail ? booking.User?.UserIdentity?.Email : null);

    private static BookingDetailResponse MapDetail(Booking booking)
        => new(
            booking.Id,
            booking.GroundId,
            booking.Ground.Name,
            booking.UserId,
            booking.User.UserIdentity.Email,
            booking.BookingDate,
            booking.StartTime,
            booking.EndTime,
            booking.PricePerHour,
            booking.TotalAmount,
            booking.AdvanceAmount,
            booking.RemainingAmount,
            booking.Status,
            booking.Notes,
            booking.Payments
                .OrderBy(p => p.CreatedAt)
                .Select(p => new BookingPaymentResponse(p.Id, p.Amount, p.Method, p.Status, p.TransactionReference, p.PaidAt))
                .ToList());
}
