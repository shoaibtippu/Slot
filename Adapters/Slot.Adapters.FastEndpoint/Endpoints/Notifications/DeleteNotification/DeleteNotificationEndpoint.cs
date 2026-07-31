using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.DeleteNotification;

[Authorize]
public class DeleteNotification(INotificationService notificationService) : EndpointWithoutRequest<DeleteNotificationEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/notifications/{id}");
        Summary(s =>
        {
            s.Summary = "Delete a notification.";
            s.Response<DeleteNotificationEndpointResponse>(200, "Notification deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Notification not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new DeleteNotificationEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new DeleteNotificationEndpointResponse { Success = false, Error = "Invalid notification id." }, 400, ct);
            return;
        }

        var result = await notificationService.DeleteAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteNotificationEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteNotificationEndpointResponse { Success = true, Error = null }, ct);
    }
}
