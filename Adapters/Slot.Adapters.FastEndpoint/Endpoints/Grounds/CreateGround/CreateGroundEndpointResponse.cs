namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.CreateGround;

public class CreateGroundEndpointResponse
{
    public bool Success { get; set; }
    public GroundDetailResponse? Ground { get; set; }
    public string? Error { get; set; }
}
