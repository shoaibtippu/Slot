namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Schedules.GetGroundSchedules;

public class GetGroundSchedules(IGroundService groundService) : EndpointWithoutRequest<GetGroundSchedulesEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/{id}/schedules");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get weekly schedule for a ground.";
            s.Response<GetGroundSchedulesEndpointResponse>(200, "Schedules retrieved successfully.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetGroundSchedulesEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.GetSchedulesAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundSchedulesEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundSchedulesEndpointResponse { Schedules = result.Value!, Error = null }, ct);
    }
}
