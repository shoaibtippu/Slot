namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetConversation;

public class GetConversationEndpointResponse
{
    public ConversationDetailResponse? Conversation { get; set; }
    public string? Error { get; set; }
}
