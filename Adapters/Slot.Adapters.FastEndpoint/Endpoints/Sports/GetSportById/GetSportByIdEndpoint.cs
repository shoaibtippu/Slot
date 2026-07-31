namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.GetSportById;

public class GetSportById(ISportService sportService) : EndpointWithoutRequest<GetSportByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/sports/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get sport detail.";
            s.Response<GetSportByIdEndpointResponse>(200, "Sport retrieved successfully.");
            s.Response(404, "Sport not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetSportByIdEndpointResponse { Error = "Invalid sport id." }, 400, ct);
            return;
        }

        var result = await sportService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetSportByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetSportByIdEndpointResponse { Sport = result.Value!, Error = null }, ct);
    }
}
