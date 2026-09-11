using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.StartDirectConversation;

[Authorize]
public class StartDirectConversation(IConversationService conversationService) : Endpoint<StartDirectConversationEndpointRequest, StartDirectConversationEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/conversations/direct");
        Summary(s =>
        {
            s.Summary = "Start or get a direct conversation with a user by their profile id.";
            s.Response<StartDirectConversationEndpointResponse>(200, "Conversation retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(400, "Invalid target user id.");
        });
    }

    public override async Task HandleAsync(StartDirectConversationEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new StartDirectConversationEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (req.TargetUserId == Guid.Empty)
        {
            await Send.ResponseAsync(new StartDirectConversationEndpointResponse { Error = "Target user id is required." }, 400, ct);
            return;
        }

        var result = await conversationService.GetOrCreateDirectAsync(identityId, req.TargetUserId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new StartDirectConversationEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new StartDirectConversationEndpointResponse { Conversation = result.Value!, Error = null }, ct);
    }
}
