namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.UpdateReview;

public class UpdateReviewEndpointResponse
{
    public bool Success { get; set; }
    public ReviewResponse? Review { get; set; }
    public string? Error { get; set; }
}
