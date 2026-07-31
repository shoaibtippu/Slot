using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Account.GetProfile;

[Authorize]
public class GetProfile(IAccountService accountService) : EndpointWithoutRequest<GetProfileEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/account/me");
        Summary(s =>
        {
            s.Summary = "Get the authenticated user's profile.";
            s.Response<GetProfileEndpointResponse>(200, "Profile retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "User not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetProfileEndpointResponse(), 401, ct);
            return;
        }

        var result = await accountService.GetProfileAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetProfileEndpointResponse(), result.Error!.Code, ct);
            return;
        }

        await Send.OkAsync(new GetProfileEndpointResponse
        {
            UserId = result.Value!.UserId,
            Email = result.Value.Email,
            FullName = result.Value.FullName,
            PhoneNumber = result.Value.PhoneNumber,
            ImageUrl = result.Value.ImageUrl,
            Roles = result.Value.Roles
        }, ct);
    }
}