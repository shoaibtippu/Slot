namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.UploadGroundImages;

public class UploadGroundImagesEndpointResponse
{
    public bool Success { get; set; }
    public IReadOnlyList<GroundImageResponse>? Images { get; set; }
    public string? Error { get; set; }
}
