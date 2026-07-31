namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.GetGroundReviews;

public class GetGroundReviewsEndpointResponse
{
    public IReadOnlyList<ReviewResponse> Reviews { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Error { get; set; }
}
