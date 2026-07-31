using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetMyConversations;

[Authorize]
public class GetMyConversations(IConversationService conversationService) : EndpointWithoutRequest<GetMyConversationsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/conversations");
        Summary(s =>
        {
            s.Summary = "Get my conversations list with last message.";
            s.Response<GetMyConversationsEndpointResponse>(200, "Conversations retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetMyConversationsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await conversationService.GetMyConversationsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetMyConversationsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetMyConversationsEndpointResponse { Conversations = result.Value!, Error = null }, ct);
    }
}
