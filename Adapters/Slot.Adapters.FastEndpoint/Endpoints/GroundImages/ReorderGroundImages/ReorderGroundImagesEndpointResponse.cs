namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.ReorderGroundImages;

public class ReorderGroundImagesEndpointResponse
{
    public bool Success { get; set; }
    public IReadOnlyList<GroundImageResponse>? Images { get; set; }
    public string? Error { get; set; }
}
