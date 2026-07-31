using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.UnblockAvailability;

[Authorize]
public class UnblockAvailability(IGroundService groundService) : EndpointWithoutRequest<UnblockAvailabilityEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/grounds/{id}/availability/{blockId}");
        Summary(s =>
        {
            s.Summary = "Unblock a time slot.";
            s.Response<UnblockAvailabilityEndpointResponse>(200, "Availability unblocked successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground or block not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UnblockAvailabilityEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var groundId) || !Guid.TryParse(Route<string>("blockId"), out var blockId))
        {
            await Send.ResponseAsync(new UnblockAvailabilityEndpointResponse { Success = false, Error = "Invalid id." }, 400, ct);
            return;
        }

        var result = await groundService.UnblockAvailabilityAsync(identityId, groundId, blockId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UnblockAvailabilityEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UnblockAvailabilityEndpointResponse { Success = true, Error = null }, ct);
    }
}
