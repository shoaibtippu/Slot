namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetMyConversations;

public class GetMyConversationsEndpointResponse
{
    public IReadOnlyList<ConversationListItemResponse> Conversations { get; set; } = [];
    public string? Error { get; set; }
}
