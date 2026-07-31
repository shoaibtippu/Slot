namespace Slot.Application.Ports.In.Grounds;

public interface IGroundService
{
    Task<Result<IReadOnlyList<GroundListItemResponse>>> GetAllAsync(GroundListRequest request, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundListItemResponse>>> GetMyGroundsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> CreateAsync(string userIdentityId, CreateGroundRequest request, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> UpdateAsync(string userIdentityId, Guid id, UpdateGroundRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userIdentityId, Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundImageResponse>>> GetImagesAsync(Guid groundId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundImageResponse>>> UploadImagesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundImageUploadRequest> images, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundImageResponse>>> ReorderImagesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundImageOrderRequest> images, CancellationToken ct = default);
    Task<Result> DeleteImageAsync(string userIdentityId, Guid groundId, Guid imageId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundScheduleResponse>>> GetSchedulesAsync(Guid groundId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundScheduleResponse>>> ReplaceSchedulesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundScheduleRequest> schedules, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundAvailabilityBlockResponse>>> BlockAvailabilityAsync(string userIdentityId, Guid groundId, GroundAvailabilityBlockRequest request, CancellationToken ct = default);
    Task<Result> UnblockAvailabilityAsync(string userIdentityId, Guid groundId, Guid blockId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundAvailabilitySlotResponse>>> GetAvailabilityAsync(Guid groundId, DateOnly date, CancellationToken ct = default);
}

public record GroundListRequest(
    string? Search,
    Guid? SportId,
    decimal? Latitude,
    decimal? Longitude,
    decimal? RadiusKm,
    decimal? MinHourlyRate,
    decimal? MaxHourlyRate);

public record CreateGroundRequest(
    string? Name,
    string? Description,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string PhoneNumber,
    string? AlternatePhoneNumber,
    decimal HourlyRate,
    decimal AdvancePercentage);

public record UpdateGroundRequest(
    string? Name,
    string? Description,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string PhoneNumber,
    string? AlternatePhoneNumber,
    decimal HourlyRate,
    decimal AdvancePercentage);

public record GroundListItemResponse(
    Guid Id,
    string? Name,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    decimal HourlyRate,
    decimal AverageRating,
    int TotalReviews,
    string? CoverImageUrl,
    IReadOnlyList<string> Sports);

public record GroundDetailResponse(
    Guid Id,
    string? Name,
    string? Description,
    string? Address,
    decimal Latitude,
    decimal Longitude,
    string PhoneNumber,
    string? AlternatePhoneNumber,
    decimal HourlyRate,
    decimal AdvancePercentage,
    decimal AverageRating,
    int TotalReviews,
    string? CoverImageUrl,
    Guid OwnerId,
    IReadOnlyList<GroundImageResponse> Images,
    IReadOnlyList<GroundSportResponse> Sports,
    IReadOnlyList<GroundScheduleResponse> Schedules);

public record GroundImageResponse(Guid Id, string ImageUrl, int DisplayOrder);
public record GroundSportResponse(Guid SportId, string Name, string? IconUrl);
public record GroundScheduleResponse(DayOfWeek DayOfWeek, TimeSpan OpeningTime, TimeSpan ClosingTime, bool IsClosed);
public record GroundScheduleRequest(DayOfWeek DayOfWeek, TimeSpan OpeningTime, TimeSpan ClosingTime, bool IsClosed);
public record GroundAvailabilityBlockRequest(DateOnly Date, TimeSpan StartTime, TimeSpan EndTime);
public record GroundAvailabilityBlockResponse(Guid Id, DateOnly Date, TimeSpan StartTime, TimeSpan EndTime, bool IsBlocked);
public record GroundAvailabilitySlotResponse(TimeSpan StartTime, TimeSpan EndTime, string Status, Guid? BookingId, Guid? AvailabilityId);