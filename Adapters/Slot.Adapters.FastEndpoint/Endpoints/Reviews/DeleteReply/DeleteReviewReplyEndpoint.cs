using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.DeleteReply;

[Authorize]
public class DeleteReviewReply(IReviewService reviewService) : EndpointWithoutRequest<DeleteReviewReplyEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/reviews/{id}/reply");
        Summary(s =>
        {
            s.Summary = "Delete the owner's reply to a review.";
            s.Response<DeleteReviewReplyEndpointResponse>(200, "Reply deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Review or reply not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new DeleteReviewReplyEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var reviewId))
        {
            await Send.ResponseAsync(new DeleteReviewReplyEndpointResponse { Success = false, Error = "Invalid review id." }, 400, ct);
            return;
        }

        var result = await reviewService.DeleteReplyAsync(identityId, reviewId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteReviewReplyEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteReviewReplyEndpointResponse { Success = true, Error = null }, ct);
    }
}
