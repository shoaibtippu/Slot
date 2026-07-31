using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.UpdateGround;

[Authorize]
public class UpdateGround(IGroundService groundService) : Endpoint<UpdateGroundEndpointRequest, UpdateGroundEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/grounds/{id}");
        Summary(s =>
        {
            s.Summary = "Update a ground.";
            s.Response<UpdateGroundEndpointResponse>(200, "Ground updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(UpdateGroundEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateGroundEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UpdateGroundEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.UpdateAsync(identityId, id, new UpdateGroundRequest(req.Name, req.Description, req.Address, req.Latitude, req.Longitude, req.PhoneNumber, req.AlternatePhoneNumber, req.HourlyRate, req.AdvancePercentage), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateGroundEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateGroundEndpointResponse { Success = true, Ground = result.Value, Error = null }, ct);
    }
}
