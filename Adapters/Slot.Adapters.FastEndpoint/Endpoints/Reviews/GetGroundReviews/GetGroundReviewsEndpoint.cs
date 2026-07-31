namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.GetGroundReviews;

public class GetGroundReviews(IReviewService reviewService) : EndpointWithoutRequest<GetGroundReviewsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/{id}/reviews");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated reviews for a ground.";
            s.Response<GetGroundReviewsEndpointResponse>(200, "Reviews retrieved successfully.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var groundId))
        {
            await Send.ResponseAsync(new GetGroundReviewsEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var query = new PagedSearchSortDto
        {
            PageNumber = Query<int?>("page_number") ?? 1,
            PageSize = Query<int?>("page_size") ?? 10,
            OrderBy = Query<string?>("order_by"),
            Search = Query<string?>("search")
        };

        var result = await reviewService.GetByGroundAsync(groundId, query, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundReviewsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundReviewsEndpointResponse
        {
            Reviews = result.Value!.Data,
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            Error = null
        }, ct);
    }
}
