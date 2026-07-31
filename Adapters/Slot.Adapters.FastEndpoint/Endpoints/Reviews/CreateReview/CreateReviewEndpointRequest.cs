namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.CreateReview;

public class CreateReviewEndpointRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
