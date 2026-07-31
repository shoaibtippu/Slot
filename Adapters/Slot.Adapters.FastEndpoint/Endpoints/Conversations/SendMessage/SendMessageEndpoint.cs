using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.SendMessage;

[Authorize]
public class SendMessage(IMessageService messageService) : Endpoint<SendMessageEndpointRequest, SendMessageEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/conversations/{id}/messages");
        Summary(s =>
        {
            s.Summary = "Send a message.";
            s.Response<SendMessageEndpointResponse>(200, "Message sent successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Conversation not found.");
        });
    }

    public override async Task HandleAsync(SendMessageEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new SendMessageEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var conversationId))
        {
            await Send.ResponseAsync(new SendMessageEndpointResponse { Error = "Invalid conversation id." }, 400, ct);
            return;
        }

        var result = await messageService.SendAsync(identityId, conversationId, new SendMessageRequest(req.Text), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new SendMessageEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new SendMessageEndpointResponse { Message = result.Value!, Error = null }, ct);
    }
}
