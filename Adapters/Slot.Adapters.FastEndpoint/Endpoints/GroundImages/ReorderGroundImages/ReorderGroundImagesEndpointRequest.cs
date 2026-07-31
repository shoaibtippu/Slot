namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.ReorderGroundImages;

public class ReorderGroundImagesEndpointRequest
{
    public List<ReorderGroundImageItem> Images { get; set; } = [];
}

public class ReorderGroundImageItem
{
    public Guid ImageId { get; set; }
    public int DisplayOrder { get; set; }
}
