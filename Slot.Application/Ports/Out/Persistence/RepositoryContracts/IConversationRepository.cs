namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IConversationRepository
{
    Task<Conversation?> FindByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<Conversation?> FindByIdAsync(Guid conversationId, CancellationToken ct = default);
    Task<IReadOnlyList<Conversation>> GetMyConversationsAsync(Guid userId, CancellationToken ct = default);
    Task CreateAsync(Conversation conversation, CancellationToken ct = default);
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
}
