namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.UpdateReview;

public class UpdateReviewEndpointRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
