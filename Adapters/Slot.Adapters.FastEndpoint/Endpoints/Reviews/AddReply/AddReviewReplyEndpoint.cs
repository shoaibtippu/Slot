using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.AddReply;

[Authorize]
public class AddReviewReply(IReviewService reviewService) : Endpoint<AddReviewReplyEndpointRequest, AddReviewReplyEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/reviews/{id}/reply");
        Summary(s =>
        {
            s.Summary = "Add a reply to a review (ground owner only).";
            s.Response<AddReviewReplyEndpointResponse>(200, "Reply added successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Review not found.");
            s.Response(409, "Reply already exists.");
        });
    }

    public override async Task HandleAsync(AddReviewReplyEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new AddReviewReplyEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var reviewId))
        {
            await Send.ResponseAsync(new AddReviewReplyEndpointResponse { Success = false, Error = "Invalid review id." }, 400, ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.Text))
        {
            await Send.ResponseAsync(new AddReviewReplyEndpointResponse { Success = false, Error = "Reply text is required." }, 400, ct);
            return;
        }

        var result = await reviewService.AddReplyAsync(identityId, reviewId, new AddReviewReplyRequest(req.Text), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new AddReviewReplyEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new AddReviewReplyEndpointResponse { Success = true, Review = result.Value, Error = null }, ct);
    }
}
