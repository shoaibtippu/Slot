using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Account.ChangePassword;

[Authorize]
public class ChangePassword(IAccountService accountService) : Endpoint<ChangePasswordEndpointRequest, ChangePasswordEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/account/change-password");
        Summary(s =>
        {
            s.Summary = "Change the authenticated user's password.";
            s.Response<ChangePasswordEndpointResponse>(200, "Password changed successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(ChangePasswordEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new ChangePasswordEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await accountService.ChangePasswordAsync(
            identityId,
            new ChangePasswordRequest(req.CurrentPassword, req.NewPassword),
            ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new ChangePasswordEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new ChangePasswordEndpointResponse { Success = true, Error = null }, ct);
    }
}