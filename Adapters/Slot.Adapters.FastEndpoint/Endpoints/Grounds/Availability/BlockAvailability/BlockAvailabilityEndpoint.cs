using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.BlockAvailability;

[Authorize]
public class BlockAvailability(IGroundService groundService) : Endpoint<BlockAvailabilityEndpointRequest, BlockAvailabilityEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/grounds/{id}/availability/block");
        Summary(s =>
        {
            s.Summary = "Block a time slot for a ground.";
            s.Response<BlockAvailabilityEndpointResponse>(200, "Availability blocked successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(BlockAvailabilityEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new BlockAvailabilityEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new BlockAvailabilityEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.BlockAvailabilityAsync(identityId, id, new GroundAvailabilityBlockRequest(req.Date, req.StartTime, req.EndTime), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new BlockAvailabilityEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new BlockAvailabilityEndpointResponse { Success = true, Blocks = result.Value, Error = null }, ct);
    }
}
