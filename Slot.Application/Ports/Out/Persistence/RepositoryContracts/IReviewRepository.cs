namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IReviewRepository
{
    Task<Review?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Review?> FindByIdWithReplyAsync(Guid id, CancellationToken ct = default);
    Task<Review?> FindByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<PagedResult<Review>> GetByGroundAsync(Guid groundId, PagedSearchSortDto query, CancellationToken ct = default);
    Task<PagedResult<Review>> GetByOwnerAsync(Guid ownerId, PagedSearchSortDto query, int? rating = null, Guid? groundId = null, CancellationToken ct = default);
    Task CreateAsync(Review review, CancellationToken ct = default);
    Task UpdateAsync(Review review, CancellationToken ct = default);
    Task DeleteAsync(Review review, CancellationToken ct = default);
    Task AddReplyAsync(ReviewReply reply, CancellationToken ct = default);
    Task UpdateReplyAsync(ReviewReply reply, CancellationToken ct = default);
    Task DeleteReplyAsync(ReviewReply reply, CancellationToken ct = default);
}
