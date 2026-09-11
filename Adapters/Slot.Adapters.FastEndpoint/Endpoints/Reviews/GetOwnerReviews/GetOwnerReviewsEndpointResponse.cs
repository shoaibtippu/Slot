namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.GetOwnerReviews;

public class GetOwnerReviewsEndpointResponse
{
    public IEnumerable<ReviewResponse>? Reviews { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Error { get; set; }
}
