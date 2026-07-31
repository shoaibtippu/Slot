using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.CreateSport;

[Authorize(Roles = "Admin")]
public class CreateSport(ISportService sportService) : Endpoint<CreateSportEndpointRequest, CreateSportEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/admin/sports");
        Summary(s =>
        {
            s.Summary = "Create a sport.";
            s.Response<CreateSportEndpointResponse>(200, "Sport created successfully.");
            s.Response(400, "Validation error.");
            s.Response(409, "Sport already exists.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CreateSportEndpointRequest req, CancellationToken ct)
    {
        var result = await sportService.CreateAsync(new CreateSportRequest(req.Name, req.IconUrl), ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CreateSportEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CreateSportEndpointResponse { Success = true, Sport = result.Value, Error = null }, ct);
    }
}