using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.DeleteReview;

[Authorize]
public class DeleteReview(IReviewService reviewService) : EndpointWithoutRequest<DeleteReviewEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/reviews/{id}");
        Summary(s =>
        {
            s.Summary = "Delete own review or admin delete.";
            s.Response<DeleteReviewEndpointResponse>(200, "Review deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Review not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new DeleteReviewEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var reviewId))
        {
            await Send.ResponseAsync(new DeleteReviewEndpointResponse { Success = false, Error = "Invalid review id." }, 400, ct);
            return;
        }

        var isAdmin = User.IsInRole("Admin");
        var result = await reviewService.DeleteAsync(identityId, reviewId, isAdmin, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteReviewEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteReviewEndpointResponse { Success = true, Error = null }, ct);
    }
}
