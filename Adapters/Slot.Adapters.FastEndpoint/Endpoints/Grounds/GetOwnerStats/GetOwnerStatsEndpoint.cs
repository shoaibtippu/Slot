using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetOwnerStats;

[Authorize]
public class GetOwnerStats(IGroundService groundService) : EndpointWithoutRequest<GetOwnerStatsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/owner-stats");
        Summary(s =>
        {
            s.Summary = "Get aggregate stats for the authenticated ground owner.";
            s.Response<GetOwnerStatsEndpointResponse>(200, "Stats retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetOwnerStatsEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await groundService.GetOwnerStatsAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetOwnerStatsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetOwnerStatsEndpointResponse { Stats = result.Value, Error = null }, ct);
    }
}
