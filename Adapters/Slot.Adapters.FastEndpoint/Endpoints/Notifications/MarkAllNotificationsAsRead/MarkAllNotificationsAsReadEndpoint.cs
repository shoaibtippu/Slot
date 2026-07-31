using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.MarkAllNotificationsAsRead;

[Authorize]
public class MarkAllNotificationsAsRead(INotificationService notificationService) : EndpointWithoutRequest<MarkAllNotificationsAsReadEndpointResponse>
{
    public override void Configure()
    {
        Patch("/api/notifications/read-all");
        Summary(s =>
        {
            s.Summary = "Mark all notifications as read.";
            s.Response<MarkAllNotificationsAsReadEndpointResponse>(200, "Notifications marked as read.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new MarkAllNotificationsAsReadEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await notificationService.MarkAllAsReadAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new MarkAllNotificationsAsReadEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new MarkAllNotificationsAsReadEndpointResponse { Success = true, Error = null }, ct);
    }
}
