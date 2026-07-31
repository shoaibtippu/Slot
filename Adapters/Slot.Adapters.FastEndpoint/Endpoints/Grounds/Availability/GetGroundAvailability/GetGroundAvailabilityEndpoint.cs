namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.GetGroundAvailability;

public class GetGroundAvailability(IGroundService groundService) : Endpoint<GetGroundAvailabilityEndpointRequest, GetGroundAvailabilityEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/{id}/availability");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get available and booked slots for a date.";
            s.Response<GetGroundAvailabilityEndpointResponse>(200, "Availability retrieved successfully.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(GetGroundAvailabilityEndpointRequest req, CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetGroundAvailabilityEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.GetAvailabilityAsync(id, req.Date, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundAvailabilityEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundAvailabilityEndpointResponse { Slots = result.Value!, Error = null }, ct);
    }
}
