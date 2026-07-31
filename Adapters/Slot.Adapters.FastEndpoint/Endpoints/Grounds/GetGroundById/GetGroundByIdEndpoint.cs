namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetGroundById;

public class GetGroundById(IGroundService groundService) : EndpointWithoutRequest<GetGroundByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get ground details.";
            s.Response<GetGroundByIdEndpointResponse>(200, "Ground retrieved successfully.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetGroundByIdEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundByIdEndpointResponse { Ground = result.Value, Error = null }, ct);
    }
}
