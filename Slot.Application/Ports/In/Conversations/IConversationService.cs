namespace Slot.Application.Ports.In.Conversations;

public interface IConversationService
{
    Task<Result<ConversationDetailResponse>> GetOrCreateByBookingAsync(string userIdentityId, Guid bookingId, CancellationToken ct = default);
    Task<Result<ConversationDetailResponse>> GetOrCreateDirectAsync(string initiatorIdentityId, Guid targetUserId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ConversationListItemResponse>>> GetMyConversationsAsync(string userIdentityId, CancellationToken ct = default);
    Task<Result<ConversationDetailResponse>> GetByIdAsync(string userIdentityId, Guid conversationId, CancellationToken ct = default);
}

public record ConversationListItemResponse(
    Guid Id,
    Guid? BookingId,
    Guid GroundOwnerId,
    Guid UserId,
    string? OtherUserName,
    string? OtherUserEmail,
    string? LastMessage,
    DateTime? LastMessageAt,
    int UnreadCount);

public record ConversationDetailResponse(
    Guid Id,
    Guid? BookingId,
    Guid GroundOwnerId,
    Guid UserId,
    string? OtherUserName,
    string? OtherUserEmail,
    IReadOnlyList<MessageResponse> Messages,
    int UnreadCount);
