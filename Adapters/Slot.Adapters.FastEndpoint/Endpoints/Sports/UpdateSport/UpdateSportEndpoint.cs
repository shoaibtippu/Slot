using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.UpdateSport;

[Authorize(Roles = "Admin")]
public class UpdateSport(ISportService sportService) : Endpoint<UpdateSportEndpointRequest, UpdateSportEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/admin/sports/{id}");
        Summary(s =>
        {
            s.Summary = "Update a sport.";
            s.Response<UpdateSportEndpointResponse>(200, "Sport updated successfully.");
            s.Response(400, "Validation error.");
            s.Response(404, "Sport not found.");
            s.Response(409, "Sport already exists.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(UpdateSportEndpointRequest req, CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UpdateSportEndpointResponse { Success = false, Error = "Invalid sport id." }, 400, ct);
            return;
        }

        var result = await sportService.UpdateAsync(id, new UpdateSportRequest(req.Name, req.IconUrl), ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UpdateSportEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UpdateSportEndpointResponse { Success = true, Sport = result.Value, Error = null }, ct);
    }
}