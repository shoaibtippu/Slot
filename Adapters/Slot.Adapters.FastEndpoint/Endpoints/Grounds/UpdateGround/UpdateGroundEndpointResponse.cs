namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.UpdateGround;

public class UpdateGroundEndpointResponse
{
    public bool Success { get; set; }
    public GroundDetailResponse? Ground { get; set; }
    public string? Error { get; set; }
}
