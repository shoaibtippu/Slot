using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.UnregisterPushToken;

[Authorize]
public class UnregisterPushToken(INotificationService notificationService) : Endpoint<UnregisterPushTokenEndpointRequest, UnregisterPushTokenEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/notifications/push/unregister");
        Summary(s =>
        {
            s.Summary = "Remove device token on logout.";
            s.Response<UnregisterPushTokenEndpointResponse>(200, "Token removed successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(UnregisterPushTokenEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UnregisterPushTokenEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await notificationService.UnregisterPushTokenAsync(identityId, new UnregisterPushTokenRequest(req.Token), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UnregisterPushTokenEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UnregisterPushTokenEndpointResponse { Success = true, Error = null }, ct);
    }
}
