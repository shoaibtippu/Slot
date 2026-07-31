using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetMyGrounds;

[Authorize]
public class GetMyGrounds(IGroundService groundService) : EndpointWithoutRequest<GetMyGroundsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/my");
        Summary(s =>
        {
            s.Summary = "Get the authenticated owner's grounds.";
            s.Response<GetMyGroundsEndpointResponse>(200, "Grounds retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetMyGroundsEndpointResponse { Grounds = [], Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await groundService.GetMyGroundsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetMyGroundsEndpointResponse { Grounds = [], Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetMyGroundsEndpointResponse { Grounds = result.Value!, Error = null }, ct);
    }
}
