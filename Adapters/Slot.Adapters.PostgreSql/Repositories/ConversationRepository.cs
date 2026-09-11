namespace Slot.Adapters.PostgreSql.Repositories;

public class ConversationRepository(ApplicationDbContext db) : IConversationRepository
{
    private static IQueryable<Conversation> WithIncludes(IQueryable<Conversation> q) =>
        q.Include(c => c.Messages)
             .ThenInclude(m => m.Sender)
                 .ThenInclude(s => s.UserIdentity)
         .Include(c => c.User)
             .ThenInclude(u => u!.UserIdentity)
         .Include(c => c.GroundOwner)
             .ThenInclude(u => u!.UserIdentity);

    public async Task<Conversation?> FindByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await WithIncludes(db.Conversations.AsNoTracking())
            .FirstOrDefaultAsync(c => c.BookingId == bookingId, ct);
    }

    public async Task<Conversation?> FindByIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await WithIncludes(db.Conversations.AsNoTracking())
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);
    }

    public async Task<Conversation?> FindDirectConversationAsync(Guid user1Id, Guid user2Id, CancellationToken ct = default)
    {
        return await WithIncludes(db.Conversations.AsNoTracking())
            .FirstOrDefaultAsync(c =>
                c.BookingId == null &&
                ((c.UserId == user1Id && c.GroundOwnerId == user2Id) ||
                 (c.UserId == user2Id && c.GroundOwnerId == user1Id)), ct);
    }

    public async Task<IReadOnlyList<Conversation>> GetMyConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await WithIncludes(db.Conversations.AsNoTracking())
            .Where(c => c.UserId == userId || c.GroundOwnerId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Conversation conversation, CancellationToken ct = default)
    {
        await db.Conversations.AddAsync(conversation, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
    {
        db.Conversations.Update(conversation);
        await db.SaveChangesAsync(ct);
    }
}
