using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Account.UpdateProfile;

[Authorize]
public class UpdateProfile(IAccountService accountService) : Endpoint<UpdateProfileEndpointRequest, UpdateProfileEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/account/me");
        Summary(s =>
        {
            s.Summary = "Update the authenticated user's profile.";
            s.Response<UpdateProfileEndpointResponse>(200, "Profile updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(UpdateProfileEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateProfileEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await accountService.UpdateProfileAsync(
            identityId,
            new UpdateProfileRequest(req.FullName, req.PhoneNumber, req.ImageUrl),
            ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateProfileEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateProfileEndpointResponse { Success = true, Error = null }, ct);
    }
}