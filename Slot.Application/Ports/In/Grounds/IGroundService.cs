namespace Slot.Application.Ports.In.Grounds;

public interface IGroundService
{
    Task<Result<IReadOnlyList<GroundListItemResponse>>> GetAllAsync(GroundListRequest request, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<GroundListItemResponse>>> GetMyGroundsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> CreateAsync(string userIdentityId, CreateGroundRequest request, CancellationToken ct = default);
    Task<Result<GroundDetailResponse>> UpdateAsync(string userIdentityId, Guid id, UpdateGroundRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userIdentityId, Guid id, CancellationToken ct = default);
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