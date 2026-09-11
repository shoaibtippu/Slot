namespace Slot.Application.Ports.In.Reviews;

public interface IReviewService
{
    Task<Result<ReviewResponse>> CreateAsync(string userIdentityId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default);
    Task<Result<PagedResult<ReviewResponse>>> GetByGroundAsync(Guid groundId, PagedSearchSortDto query, CancellationToken ct = default);
    Task<Result<PagedResult<ReviewResponse>>> GetOwnerReviewsAsync(string ownerIdentityId, PagedSearchSortDto query, int? rating = null, Guid? groundId = null, CancellationToken ct = default);
    Task<Result<ReviewResponse>> UpdateAsync(string userIdentityId, Guid reviewId, UpdateReviewRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userIdentityId, Guid reviewId, bool isAdmin, CancellationToken ct = default);
    Task<Result<ReviewResponse>> AddReplyAsync(string ownerIdentityId, Guid reviewId, AddReviewReplyRequest request, CancellationToken ct = default);
    Task<Result<ReviewResponse>> UpdateReplyAsync(string ownerIdentityId, Guid reviewId, UpdateReviewReplyRequest request, CancellationToken ct = default);
    Task<Result> DeleteReplyAsync(string ownerIdentityId, Guid reviewId, CancellationToken ct = default);
}

public record CreateReviewRequest(int Rating, string? Comment);
public record UpdateReviewRequest(int Rating, string? Comment);
public record AddReviewReplyRequest(string Text);
public record UpdateReviewReplyRequest(string Text);

public record ReviewReplyResponse(Guid Id, string Text, string? OwnerEmail, DateTime? CreatedAt, DateTime? ModifiedAt);

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
    DateTime? ModifiedAt,
    ReviewReplyResponse? Reply = null);
