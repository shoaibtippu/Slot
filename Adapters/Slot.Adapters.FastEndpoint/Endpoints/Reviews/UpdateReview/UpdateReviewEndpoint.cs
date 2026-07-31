using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.UpdateReview;

[Authorize]
public class UpdateReview(IReviewService reviewService) : Endpoint<UpdateReviewEndpointRequest, UpdateReviewEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/reviews/{id}");
        Summary(s =>
        {
            s.Summary = "Edit own review.";
            s.Response<UpdateReviewEndpointResponse>(200, "Review updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Review not found.");
        });
    }

    public override async Task HandleAsync(UpdateReviewEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateReviewEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var reviewId))
        {
            await Send.ResponseAsync(new UpdateReviewEndpointResponse { Success = false, Error = "Invalid review id." }, 400, ct);
            return;
        }

        var result = await reviewService.UpdateAsync(identityId, reviewId, new UpdateReviewRequest(req.Rating, req.Comment), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateReviewEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateReviewEndpointResponse { Success = true, Review = result.Value, Error = null }, ct);
    }
}
