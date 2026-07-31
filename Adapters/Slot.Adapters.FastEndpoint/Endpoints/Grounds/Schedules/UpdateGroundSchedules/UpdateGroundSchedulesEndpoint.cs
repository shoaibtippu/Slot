using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Schedules.UpdateGroundSchedules;

[Authorize]
public class UpdateGroundSchedules(IGroundService groundService) : Endpoint<UpdateGroundSchedulesEndpointRequest, UpdateGroundSchedulesEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/grounds/{id}/schedules");
        Summary(s =>
        {
            s.Summary = "Replace weekly opening hours for a ground.";
            s.Response<UpdateGroundSchedulesEndpointResponse>(200, "Schedules updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(UpdateGroundSchedulesEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UpdateGroundSchedulesEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UpdateGroundSchedulesEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.ReplaceSchedulesAsync(identityId, id, req.Schedules.Select(s => new GroundScheduleRequest(s.DayOfWeek, s.OpeningTime, s.ClosingTime, s.IsClosed)).ToList(), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateGroundSchedulesEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateGroundSchedulesEndpointResponse { Success = true, Schedules = result.Value, Error = null }, ct);
    }
}
