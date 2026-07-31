using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.MarkNotificationAsRead;

[Authorize]
public class MarkNotificationAsRead(INotificationService notificationService) : EndpointWithoutRequest<MarkNotificationAsReadEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/notifications/{id}/read");
        Summary(s =>
        {
            s.Summary = "Mark one notification as read.";
            s.Response<MarkNotificationAsReadEndpointResponse>(200, "Notification marked as read.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Notification not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new MarkNotificationAsReadEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new MarkNotificationAsReadEndpointResponse { Success = false, Error = "Invalid notification id." }, 400, ct);
            return;
        }

        var result = await notificationService.MarkAsReadAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new MarkNotificationAsReadEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new MarkNotificationAsReadEndpointResponse { Success = true, Error = null }, ct);
    }
}
