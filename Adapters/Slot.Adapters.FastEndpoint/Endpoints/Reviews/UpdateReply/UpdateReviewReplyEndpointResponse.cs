namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.UpdateReply;

public class UpdateReviewReplyEndpointResponse
{
    public bool Success { get; set; }
    public ReviewResponse? Review { get; set; }
    public string? Error { get; set; }
}
