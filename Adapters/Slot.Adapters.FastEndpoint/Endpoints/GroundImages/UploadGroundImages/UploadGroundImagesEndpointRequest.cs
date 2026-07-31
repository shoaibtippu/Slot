using Microsoft.AspNetCore.Http;

namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.UploadGroundImages;

public class UploadGroundImagesEndpointRequest
{
    public List<IFormFile> Images { get; set; } = [];
}
