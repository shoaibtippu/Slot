namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetOrCreateConversation;

public class GetOrCreateConversationEndpointResponse
{
    public ConversationDetailResponse? Conversation { get; set; }
    public string? Error { get; set; }
}
