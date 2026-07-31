namespace Slot.Adapters.PostgreSql.Repositories;

public class ConversationRepository(ApplicationDbContext db) : IConversationRepository
{
    public async Task<Conversation?> FindByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await db.Conversations
            .AsNoTracking()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                    .ThenInclude(s => s.UserIdentity)
            .FirstOrDefaultAsync(c => c.BookingId == bookingId, ct);
    }

    public async Task<Conversation?> FindByIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await db.Conversations
            .AsNoTracking()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                    .ThenInclude(s => s.UserIdentity)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);
    }

    public async Task<IReadOnlyList<Conversation>> GetMyConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Conversations
            .AsNoTracking()
            .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                    .ThenInclude(s => s.UserIdentity)
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
