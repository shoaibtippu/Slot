using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Conversations.GetMessages;

[Authorize]
public class GetMessages(IMessageService messageService) : EndpointWithoutRequest<GetMessagesEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/conversations/{id}/messages");
        Summary(s =>
        {
            s.Summary = "Get messages paginated, newest first.";
            s.Response<GetMessagesEndpointResponse>(200, "Messages retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Conversation not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetMessagesEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var conversationId))
        {
            await Send.ResponseAsync(new GetMessagesEndpointResponse { Error = "Invalid conversation id." }, 400, ct);
            return;
        }

        var query = new PagedSearchSortDto
        {
            PageNumber = Query<int?>("page_number") ?? 1,
            PageSize = Query<int?>("page_size") ?? 20,
            OrderBy = Query<string?>("order_by"),
            Search = Query<string?>("search")
        };

        var result = await messageService.GetMessagesAsync(identityId, conversationId, query, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetMessagesEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetMessagesEndpointResponse
        {
            Messages = result.Value!.Data,
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            Error = null
        }, ct);
    }
}
