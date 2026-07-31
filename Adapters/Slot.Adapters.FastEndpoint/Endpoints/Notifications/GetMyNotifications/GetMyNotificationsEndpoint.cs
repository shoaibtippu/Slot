using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.GetMyNotifications;

[Authorize]
public class GetMyNotifications(INotificationService notificationService) : EndpointWithoutRequest<GetMyNotificationsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/notifications");
        Summary(s =>
        {
            s.Summary = "Get my notifications list.";
            s.Response<GetMyNotificationsEndpointResponse>(200, "Notifications retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetMyNotificationsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await notificationService.GetMyNotificationsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetMyNotificationsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetMyNotificationsEndpointResponse { Notifications = result.Value!, Error = null }, ct);
    }
}
