using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.GetOwnerReviews;

[Authorize]
public class GetOwnerReviews(IReviewService reviewService) : EndpointWithoutRequest<GetOwnerReviewsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/reviews/owner");
        Summary(s =>
        {
            s.Summary = "Get paginated reviews for all grounds owned by the current user.";
            s.Response<GetOwnerReviewsEndpointResponse>(200, "Reviews retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetOwnerReviewsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var query = new PagedSearchSortDto
        {
            PageNumber = Query<int?>("page_number", isRequired: false) ?? 1,
            PageSize = Query<int?>("page_size", isRequired: false) ?? 20,
            OrderBy = Query<string?>("order_by", isRequired: false),
            Search = Query<string?>("search", isRequired: false)
        };

        var rating = Query<int?>("rating", isRequired: false);
        var groundIdStr = Query<string?>("ground_id", isRequired: false);
        Guid? groundId = Guid.TryParse(groundIdStr, out var gid) ? gid : null;

        var result = await reviewService.GetOwnerReviewsAsync(identityId, query, rating, groundId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetOwnerReviewsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetOwnerReviewsEndpointResponse
        {
            Reviews = result.Value!.Data,
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            Error = null
        }, ct);
    }
}
