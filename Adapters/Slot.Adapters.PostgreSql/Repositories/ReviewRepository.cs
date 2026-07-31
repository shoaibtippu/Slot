namespace Slot.Adapters.PostgreSql.Repositories;

public class ReviewRepository(ApplicationDbContext db) : IReviewRepository
{
    public async Task<Review?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Reviews
            .AsNoTracking()
            .Include(r => r.Ground)
            .Include(r => r.User)
                .ThenInclude(u => u.UserIdentity)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Review?> FindByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await db.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.BookingId == bookingId, ct);
    }

    public async Task<PagedResult<Review>> GetByGroundAsync(Guid groundId, PagedSearchSortDto query, CancellationToken ct = default)
    {
        var baseQuery = db.Reviews
            .AsNoTracking()
            .Include(r => r.Ground)
            .Include(r => r.User)
                .ThenInclude(u => u.UserIdentity)
            .Where(r => r.GroundId == groundId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(r =>
                (r.Comment != null && r.Comment.Contains(search)) ||
                (r.User.UserIdentity.Email != null && r.User.UserIdentity.Email.Contains(search)));
        }

        var totalCount = await baseQuery.CountAsync(ct);
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var data = await ApplyOrdering(baseQuery, query.OrderBy)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<Review>.Success(data, totalCount, pageNumber, pageSize);
    }

    public async Task CreateAsync(Review review, CancellationToken ct = default)
    {
        await db.Reviews.AddAsync(review, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Review review, CancellationToken ct = default)
    {
        db.Reviews.Update(review);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Review review, CancellationToken ct = default)
    {
        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
    }

    private static IQueryable<Review> ApplyOrdering(IQueryable<Review> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderByDescending(r => r.CreatedAt);

        return orderBy.Trim().ToLowerInvariant() switch
        {
            "rating" => query.OrderByDescending(r => r.Rating),
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "newest" => query.OrderByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };
    }
}
