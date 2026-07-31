namespace Slot.Application.Services;

public class ConversationService(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IBookingRepository bookingRepository,
    IUserRepository userRepository) : IConversationService
{
    public async Task<Result<ConversationDetailResponse>> GetOrCreateByBookingAsync(string userIdentityId, Guid bookingId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ConversationDetailResponse>(Error.NotFound("User not found."));

        var booking = await bookingRepository.FindByIdAsync(bookingId, ct);
        if (booking is null)
            return Result.Failure<ConversationDetailResponse>(Error.NotFound("Booking not found."));

        if (booking.UserId != profile.Id && booking.Ground.OwnerId != profile.Id)
            return Result.Failure<ConversationDetailResponse>(Error.Validation("You are not allowed to access this conversation."));

        var conversation = await conversationRepository.FindByBookingIdAsync(bookingId, ct);
        if (conversation is null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                GroundOwnerId = booking.Ground.OwnerId,
                UserId = booking.UserId
            };
            await conversationRepository.CreateAsync(conversation, ct);
        }

        return Result.Success(MapDetail(conversation, profile.Id));
    }

    public async Task<Result<IReadOnlyList<ConversationListItemResponse>>> GetMyConversationsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<ConversationListItemResponse>>(Error.NotFound("User not found."));

        var conversations = await conversationRepository.GetMyConversationsAsync(profile.Id, ct);
        var list = conversations.Select(c => MapListItem(c, profile.Id)).ToList();
        return Result.Success<IReadOnlyList<ConversationListItemResponse>>(list);
    }

    public async Task<Result<ConversationDetailResponse>> GetByIdAsync(string userIdentityId, Guid conversationId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<ConversationDetailResponse>(Error.NotFound("User not found."));

        var conversation = await conversationRepository.FindByIdAsync(conversationId, ct);
        if (conversation is null)
            return Result.Failure<ConversationDetailResponse>(Error.NotFound("Conversation not found."));

        if (conversation.UserId != profile.Id && conversation.GroundOwnerId != profile.Id)
            return Result.Failure<ConversationDetailResponse>(Error.Validation("You are not allowed to access this conversation."));

        return Result.Success(MapDetail(conversation, profile.Id));
    }

    private static ConversationListItemResponse MapListItem(Conversation conversation, Guid currentUserId)
    {
        var lastMessage = conversation.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        var unreadCount = conversation.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);
        return new ConversationListItemResponse(
            conversation.Id,
            conversation.BookingId,
            conversation.GroundOwnerId,
            conversation.UserId,
            lastMessage?.Text,
            lastMessage?.CreatedAt,
            unreadCount);
    }

    private static ConversationDetailResponse MapDetail(Conversation conversation, Guid currentUserId)
    {
        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageResponse(
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.Sender?.UserIdentity?.UserName ?? m.Sender?.UserIdentity?.Email,
                m.Text,
                m.IsRead,
                m.CreatedAt))
            .ToList();

        var unreadCount = conversation.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);
        return new ConversationDetailResponse(conversation.Id, conversation.BookingId, conversation.GroundOwnerId, conversation.UserId, messages, unreadCount);
    }
}
