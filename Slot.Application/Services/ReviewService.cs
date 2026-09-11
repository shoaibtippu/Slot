namespace Slot.Application.Services;

public class ReviewService(IReviewRepository reviewRepository, IBookingRepository bookingRepository, IUserRepository userRepository) : IReviewService
{
    public async Task<Result<ReviewResponse>> CreateAsync(string userIdentityId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdWithDetailsAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id)
            return Result.Failure<ReviewResponse>(Error.UnAuthorized("You are not allowed to review this booking."));

        if (booking.Status != BookingStatus.Completed)
            return Result.Failure<ReviewResponse>(Error.Validation("Only completed bookings can be reviewed."));

        if (request.Rating is < 1 or > 5)
            return Result.Failure<ReviewResponse>(Error.Validation("Rating must be between 1 and 5."));

        var existing = await reviewRepository.FindByBookingIdAsync(bookingId, ct);
        if (existing is not null)
            return Result.Failure<ReviewResponse>(Error.Conflict("A review for this booking already exists."));

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            GroundId = booking.GroundId,
            UserId = profile.Id,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };

        await reviewRepository.CreateAsync(review, ct);
        var created = await reviewRepository.FindByIdAsync(review.Id, ct);
        return created is null ? Result.Failure<ReviewResponse>(Error.NotFound("Review not found.")) : Result.Success(Map(created));
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetByGroundAsync(Guid groundId, PagedSearchSortDto query, CancellationToken ct = default)
    {
        var paged = await reviewRepository.GetByGroundAsync(groundId, query, ct);
        return Result.Success(PagedResult<ReviewResponse>.Success(paged.Data.Select(Map).ToList(), paged.TotalCount, paged.PageNumber, paged.PageSize));
    }

    public async Task<Result<PagedResult<ReviewResponse>>> GetOwnerReviewsAsync(string ownerIdentityId, PagedSearchSortDto query, int? rating = null, Guid? groundId = null, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(ownerIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<PagedResult<ReviewResponse>>(Error.NotFound("User not found."));

        var paged = await reviewRepository.GetByOwnerAsync(profile.Id, query, rating, groundId, ct);
        return Result.Success(PagedResult<ReviewResponse>.Success(paged.Data.Select(Map).ToList(), paged.TotalCount, paged.PageNumber, paged.PageSize));
    }

    public async Task<Result<ReviewResponse>> UpdateAsync(string userIdentityId, Guid reviewId, UpdateReviewRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("User not found."));

        var review = await reviewRepository.FindByIdAsync(reviewId, ct);
        if (review is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Review not found."));

        if (review.UserId != profile.Id)
            return Result.Failure<ReviewResponse>(Error.UnAuthorized("You are not allowed to edit this review."));

        if (request.Rating is < 1 or > 5)
            return Result.Failure<ReviewResponse>(Error.Validation("Rating must be between 1 and 5."));

        review.Rating = request.Rating;
        review.Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        await reviewRepository.UpdateAsync(review, ct);

        var updated = await reviewRepository.FindByIdAsync(review.Id, ct);
        return updated is null ? Result.Failure<ReviewResponse>(Error.NotFound("Review not found.")) : Result.Success(Map(updated));
    }

    public async Task<Result> DeleteAsync(string userIdentityId, Guid reviewId, bool isAdmin, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var review = await reviewRepository.FindByIdAsync(reviewId, ct);
        if (review is null)
            return Result.Failure(Error.NotFound("Review not found."));

        if (!isAdmin && review.UserId != profile.Id)
            return Result.Failure(Error.UnAuthorized("You are not allowed to delete this review."));

        await reviewRepository.DeleteAsync(review, ct);
        return Result.Success();
    }

    public async Task<Result<ReviewResponse>> AddReplyAsync(string ownerIdentityId, Guid reviewId, AddReviewReplyRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return Result.Failure<ReviewResponse>(Error.Validation("Reply text is required."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(ownerIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("User not found."));

        var review = await reviewRepository.FindByIdWithReplyAsync(reviewId, ct);
        if (review is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Review not found."));

        if (review.Ground?.OwnerId != profile.Id)
            return Result.Failure<ReviewResponse>(Error.UnAuthorized("You are not allowed to reply to this review."));

        if (review.Reply is not null)
            return Result.Failure<ReviewResponse>(Error.Conflict("A reply already exists for this review. Use update to change it."));

        var reply = new ReviewReply
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            OwnerId = profile.Id,
            Text = request.Text.Trim()
        };

        await reviewRepository.AddReplyAsync(reply, ct);

        var updated = await reviewRepository.FindByIdAsync(reviewId, ct);
        return updated is null ? Result.Failure<ReviewResponse>(Error.NotFound("Review not found.")) : Result.Success(Map(updated));
    }

    public async Task<Result<ReviewResponse>> UpdateReplyAsync(string ownerIdentityId, Guid reviewId, UpdateReviewReplyRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return Result.Failure<ReviewResponse>(Error.Validation("Reply text is required."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(ownerIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("User not found."));

        var review = await reviewRepository.FindByIdWithReplyAsync(reviewId, ct);
        if (review is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("Review not found."));

        if (review.Reply is null)
            return Result.Failure<ReviewResponse>(Error.NotFound("No reply exists for this review."));

        if (review.Reply.OwnerId != profile.Id)
            return Result.Failure<ReviewResponse>(Error.UnAuthorized("You are not allowed to edit this reply."));

        review.Reply.Text = request.Text.Trim();
        await reviewRepository.UpdateReplyAsync(review.Reply, ct);

        var updated = await reviewRepository.FindByIdAsync(reviewId, ct);
        return updated is null ? Result.Failure<ReviewResponse>(Error.NotFound("Review not found.")) : Result.Success(Map(updated));
    }

    public async Task<Result> DeleteReplyAsync(string ownerIdentityId, Guid reviewId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(ownerIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var review = await reviewRepository.FindByIdWithReplyAsync(reviewId, ct);
        if (review is null)
            return Result.Failure(Error.NotFound("Review not found."));

        if (review.Reply is null)
            return Result.Failure(Error.NotFound("No reply exists for this review."));

        if (review.Reply.OwnerId != profile.Id)
            return Result.Failure(Error.UnAuthorized("You are not allowed to delete this reply."));

        await reviewRepository.DeleteReplyAsync(review.Reply, ct);
        return Result.Success();
    }

    private static ReviewReplyResponse? MapReply(ReviewReply? reply)
    {
        if (reply is null) return null;
        return new ReviewReplyResponse(reply.Id, reply.Text, reply.Owner?.UserIdentity?.Email, reply.CreatedAt, reply.ModifiedAt);
    }

    private static ReviewResponse Map(Review review)
        => new(review.Id, review.GroundId, review.Ground?.Name, review.BookingId, review.UserId, review.User?.UserIdentity?.Email, review.Rating, review.Comment, review.CreatedAt, review.ModifiedAt, MapReply(review.Reply));
}
