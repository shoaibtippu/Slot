using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.UpdateReply;

[Authorize]
public class UpdateReviewReply(IReviewService reviewService) : Endpoint<UpdateReviewReplyEndpointRequest, UpdateReviewReplyEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/reviews/{id}/reply");
        Summary(s =>
        {
            s.Summary = "Update the owner's reply to a review.";
            s.Response<UpdateReviewReplyEndpointResponse>(200, "Reply updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Review or reply not found.");
        });
    }

    public override async Task HandleAsync(UpdateReviewReplyEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateReviewReplyEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var reviewId))
        {
            await Send.ResponseAsync(new UpdateReviewReplyEndpointResponse { Success = false, Error = "Invalid review id." }, 400, ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.Text))
        {
            await Send.ResponseAsync(new UpdateReviewReplyEndpointResponse { Success = false, Error = "Reply text is required." }, 400, ct);
            return;
        }

        var result = await reviewService.UpdateReplyAsync(identityId, reviewId, new UpdateReviewReplyRequest(req.Text), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateReviewReplyEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateReviewReplyEndpointResponse { Success = true, Review = result.Value, Error = null }, ct);
    }
}
