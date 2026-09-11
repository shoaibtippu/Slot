namespace Slot.Adapters.PostgreSql.Repositories;

public class MessageRepository(ApplicationDbContext db) : IMessageRepository
{
    public async Task<IReadOnlyList<Message>> GetByConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await db.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
                .ThenInclude(s => s.UserIdentity)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Message>> GetPagedByConversationAsync(Guid conversationId, PagedSearchSortDto query, CancellationToken ct = default)
    {
        var baseQuery = db.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
                .ThenInclude(s => s.UserIdentity)
            .Where(m => m.ConversationId == conversationId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            baseQuery = baseQuery.Where(m => m.Text.Contains(search) || (m.Sender.UserIdentity.Email != null && m.Sender.UserIdentity.Email.Contains(search)));
        }

        var totalCount = await baseQuery.CountAsync(ct);
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var data = await baseQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return PagedResult<Message>.Success(data, totalCount, pageNumber, pageSize);
    }

    public async Task<Message?> FindByIdAsync(Guid messageId, CancellationToken ct = default)
    {
        return await db.Messages
            .Include(m => m.Conversation)
            .FirstOrDefaultAsync(m => m.Id == messageId, ct);
    }

    public async Task CreateAsync(Message message, CancellationToken ct = default)
    {
        await db.Messages.AddAsync(message, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        // ExecuteUpdateAsync bypasses change tracking and concurrency tokens — direct SQL UPDATE
        await db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }
}
