namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.GetGroundImages;

public class GetGroundImages(IGroundService groundService) : EndpointWithoutRequest<GetGroundImagesEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/grounds/{id}/images");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all images for a ground.";
            s.Response<GetGroundImagesEndpointResponse>(200, "Ground images retrieved successfully.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetGroundImagesEndpointResponse { Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.GetImagesAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetGroundImagesEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetGroundImagesEndpointResponse { Images = result.Value!, Error = null }, ct);
    }
}
