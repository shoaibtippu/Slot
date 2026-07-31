namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetGrounds;

public class GetGrounds(IGroundService groundService) : Endpoint<GetGroundsEndpointRequest, GetGroundsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List grounds with optional search, filter, and geo constraints.";
            s.Response<GetGroundsEndpointResponse>(200, "Grounds retrieved successfully.");
        });
    }

    public override async Task HandleAsync(GetGroundsEndpointRequest req, CancellationToken ct)
    {
        var result = await groundService.GetAllAsync(
            new GroundListRequest(req.Search, req.SportId, req.Latitude, req.Longitude, req.RadiusKm, req.MinHourlyRate, req.MaxHourlyRate),
            ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundsEndpointResponse { Grounds = [], Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundsEndpointResponse { Grounds = result.Value!, Error = null }, ct);
    }
}
