namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetMessages;

public class GetMessagesEndpointResponse
{
    public IReadOnlyList<MessageResponse> Messages { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Error { get; set; }
}
