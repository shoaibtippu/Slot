namespace Slot.Application.Ports.In.Reviews;

public interface IReviewService
{
    Task<Result<ReviewResponse>> CreateAsync(string userIdentityId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default);
    Task<Result<PagedResult<ReviewResponse>>> GetByGroundAsync(Guid groundId, PagedSearchSortDto query, CancellationToken ct = default);
    Task<Result<ReviewResponse>> UpdateAsync(string userIdentityId, Guid reviewId, UpdateReviewRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userIdentityId, Guid reviewId, bool isAdmin, CancellationToken ct = default);
}

public record CreateReviewRequest(int Rating, string? Comment);
public record UpdateReviewRequest(int Rating, string? Comment);

public record ReviewResponse(
    Guid Id,
    Guid GroundId,
    string? GroundName,
    Guid BookingId,
    Guid UserId,
    string? UserEmail,
    int Rating,
    string? Comment,
    DateTime? CreatedAt,
    DateTime? ModifiedAt);
