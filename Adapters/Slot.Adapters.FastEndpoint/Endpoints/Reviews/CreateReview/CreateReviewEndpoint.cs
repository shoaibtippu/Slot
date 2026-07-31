using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Reviews.CreateReview;

[Authorize]
public class CreateReview(IReviewService reviewService) : Endpoint<CreateReviewEndpointRequest, CreateReviewEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/bookings/{id}/review");
        Summary(s =>
        {
            s.Summary = "Leave a review for a completed booking.";
            s.Response<CreateReviewEndpointResponse>(200, "Review created successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CreateReviewEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CreateReviewEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var bookingId))
        {
            await Send.ResponseAsync(new CreateReviewEndpointResponse { Success = false, Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await reviewService.CreateAsync(identityId, bookingId, new CreateReviewRequest(req.Rating, req.Comment), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CreateReviewEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CreateReviewEndpointResponse { Success = true, Review = result.Value, Error = null }, ct);
    }
}
