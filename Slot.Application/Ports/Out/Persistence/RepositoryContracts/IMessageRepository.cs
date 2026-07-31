namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetByConversationAsync(Guid conversationId, CancellationToken ct = default);
    Task<PagedResult<Message>> GetPagedByConversationAsync(Guid conversationId, PagedSearchSortDto query, CancellationToken ct = default);
    Task<Message?> FindByIdAsync(Guid messageId, CancellationToken ct = default);
    Task CreateAsync(Message message, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
}
