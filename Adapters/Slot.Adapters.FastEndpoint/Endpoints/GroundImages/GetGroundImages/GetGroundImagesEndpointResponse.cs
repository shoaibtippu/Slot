namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.GetGroundImages;

public class GetGroundImagesEndpointResponse
{
    public IReadOnlyList<GroundImageResponse> Images { get; set; } = [];
    public string? Error { get; set; }
}
