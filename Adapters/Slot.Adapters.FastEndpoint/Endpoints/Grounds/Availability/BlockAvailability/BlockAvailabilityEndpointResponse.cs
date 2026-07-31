namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.BlockAvailability;

public class BlockAvailabilityEndpointResponse
{
    public bool Success { get; set; }
    public IReadOnlyList<GroundAvailabilityBlockResponse>? Blocks { get; set; }
    public string? Error { get; set; }
}
