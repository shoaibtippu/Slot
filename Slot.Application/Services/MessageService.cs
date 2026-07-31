namespace Slot.Application.Services;

public class MessageService(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository) : IMessageService
{
    public async Task<Result<MessageResponse>> SendAsync(string userIdentityId, Guid conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        var text = request.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<MessageResponse>(Error.Validation("Message text is required."));

        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<MessageResponse>(Error.NotFound("User not found."));

        var conversation = await conversationRepository.FindByIdAsync(conversationId, ct);
        if (conversation is null)
            return Result.Failure<MessageResponse>(Error.NotFound("Conversation not found."));

        if (conversation.UserId != profile.Id && conversation.GroundOwnerId != profile.Id)
            return Result.Failure<MessageResponse>(Error.Validation("You are not allowed to access this conversation."));

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = profile.Id,
            Text = text,
            IsRead = false
        };

        await messageRepository.CreateAsync(message, ct);
        return Result.Success(Map(message, identity.UserName ?? identity.Email));
    }

    public async Task<Result<PagedResult<MessageResponse>>> GetMessagesAsync(string userIdentityId, Guid conversationId, PagedSearchSortDto query, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<PagedResult<MessageResponse>>(Error.NotFound("User not found."));

        var conversation = await conversationRepository.FindByIdAsync(conversationId, ct);
        if (conversation is null)
            return Result.Failure<PagedResult<MessageResponse>>(Error.NotFound("Conversation not found."));

        if (conversation.UserId != profile.Id && conversation.GroundOwnerId != profile.Id)
            return Result.Failure<PagedResult<MessageResponse>>(Error.Validation("You are not allowed to access this conversation."));

        var page = await messageRepository.GetPagedByConversationAsync(conversationId, query, ct);
        var mapped = page.Data.Select(m => Map(m, m.Sender?.UserIdentity?.UserName ?? m.Sender?.UserIdentity?.Email)).ToList();
        return Result.Success(PagedResult<MessageResponse>.Success(mapped, page.TotalCount, page.PageNumber, page.PageSize));
    }

    public async Task<Result> MarkAllAsReadAsync(string userIdentityId, Guid conversationId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var conversation = await conversationRepository.FindByIdAsync(conversationId, ct);
        if (conversation is null)
            return Result.Failure(Error.NotFound("Conversation not found."));

        if (conversation.UserId != profile.Id && conversation.GroundOwnerId != profile.Id)
            return Result.Failure(Error.Validation("You are not allowed to access this conversation."));

        await messageRepository.MarkAllAsReadAsync(conversationId, profile.Id, ct);
        return Result.Success();
    }

    private static MessageResponse Map(Message message, string? senderName)
        => new(message.Id, message.ConversationId, message.SenderId, senderName, message.Text, message.IsRead, message.CreatedAt);
}
