namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.CreateReview;

public class CreateReviewEndpointResponse
{
    public bool Success { get; set; }
    public ReviewResponse? Review { get; set; }
    public string? Error { get; set; }
}
