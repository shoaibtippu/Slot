using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetConversation;

[Authorize]
public class GetConversation(IConversationService conversationService) : EndpointWithoutRequest<GetConversationEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/conversations/{id}");
        Summary(s =>
        {
            s.Summary = "Single conversation detail.";
            s.Response<GetConversationEndpointResponse>(200, "Conversation retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Conversation not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetConversationEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var conversationId))
        {
            await Send.ResponseAsync(new GetConversationEndpointResponse { Error = "Invalid conversation id." }, 400, ct);
            return;
        }

        var result = await conversationService.GetByIdAsync(identityId, conversationId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetConversationEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetConversationEndpointResponse { Conversation = result.Value!, Error = null }, ct);
    }
}
