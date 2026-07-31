namespace Slot.Application.Ports.In.Conversations;

public interface IMessageService
{
    Task<Result<MessageResponse>> SendAsync(string userIdentityId, Guid conversationId, SendMessageRequest request, CancellationToken ct = default);
    Task<Result<PagedResult<MessageResponse>>> GetMessagesAsync(string userIdentityId, Guid conversationId, PagedSearchSortDto query, CancellationToken ct = default);
    Task<Result> MarkAllAsReadAsync(string userIdentityId, Guid conversationId, CancellationToken ct = default);
}

public record SendMessageRequest(string Text);

public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string? SenderName,
    string Text,
    bool IsRead,
    DateTime? CreatedAt);
