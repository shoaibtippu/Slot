using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetOrCreateConversation;

[Authorize]
public class GetOrCreateConversation(IConversationService conversationService) : EndpointWithoutRequest<GetOrCreateConversationEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/bookings/{id}/conversation");
        Summary(s =>
        {
            s.Summary = "Start or get conversation for a booking.";
            s.Response<GetOrCreateConversationEndpointResponse>(200, "Conversation retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Booking not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetOrCreateConversationEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var bookingId))
        {
            await Send.ResponseAsync(new GetOrCreateConversationEndpointResponse { Error = "Invalid booking id." }, 400, ct);
            return;
        }

        var result = await conversationService.GetOrCreateByBookingAsync(identityId, bookingId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetOrCreateConversationEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetOrCreateConversationEndpointResponse { Conversation = result.Value!, Error = null }, ct);
    }
}
