namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.AddReply;

public class AddReviewReplyEndpointResponse
{
    public bool Success { get; set; }
    public ReviewResponse? Review { get; set; }
    public string? Error { get; set; }
}
