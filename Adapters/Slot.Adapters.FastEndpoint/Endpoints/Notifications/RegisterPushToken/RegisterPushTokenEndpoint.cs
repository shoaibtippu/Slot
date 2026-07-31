using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Notifications.RegisterPushToken;

[Authorize]
public class RegisterPushToken(INotificationService notificationService) : Endpoint<RegisterPushTokenEndpointRequest, RegisterPushTokenEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/notifications/push/register");
        Summary(s =>
        {
            s.Summary = "Register FCM / APNs device token.";
            s.Response<RegisterPushTokenEndpointResponse>(200, "Token registered successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(RegisterPushTokenEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new RegisterPushTokenEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await notificationService.RegisterPushTokenAsync(identityId, new RegisterPushTokenRequest(req.Token, req.Platform), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new RegisterPushTokenEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new RegisterPushTokenEndpointResponse { Success = true, Error = null }, ct);
    }
}
