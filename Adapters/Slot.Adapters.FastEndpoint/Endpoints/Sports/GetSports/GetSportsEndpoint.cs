namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.GetSports;

public class GetSports(ISportService sportService) : EndpointWithoutRequest<GetSportsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/sports");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all sports.";
            s.Response<GetSportsEndpointResponse>(200, "Sports retrieved successfully.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await sportService.GetAllAsync(ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetSportsEndpointResponse { Sports = [], Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetSportsEndpointResponse { Sports = result.Value!, Error = null }, ct);
    }
}