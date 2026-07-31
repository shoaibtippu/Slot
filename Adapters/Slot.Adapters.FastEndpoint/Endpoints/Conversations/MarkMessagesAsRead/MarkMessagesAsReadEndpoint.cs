using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.MarkMessagesAsRead;

[Authorize]
public class MarkMessagesAsRead(IMessageService messageService) : EndpointWithoutRequest<MarkMessagesAsReadEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/conversations/{id}/messages/read");
        Summary(s =>
        {
            s.Summary = "Mark all messages in conversation as read.";
            s.Response<MarkMessagesAsReadEndpointResponse>(200, "Messages marked as read.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Conversation not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new MarkMessagesAsReadEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var conversationId))
        {
            await Send.ResponseAsync(new MarkMessagesAsReadEndpointResponse { Success = false, Error = "Invalid conversation id." }, 400, ct);
            return;
        }

        var result = await messageService.MarkAllAsReadAsync(identityId, conversationId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new MarkMessagesAsReadEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new MarkMessagesAsReadEndpointResponse { Success = true, Error = null }, ct);
    }
}
