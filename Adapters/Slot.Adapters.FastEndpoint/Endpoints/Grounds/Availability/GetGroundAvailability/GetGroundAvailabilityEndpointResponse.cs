namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.GetGroundAvailability;

public class GetGroundAvailabilityEndpointResponse
{
    public IReadOnlyList<GroundAvailabilitySlotResponse> Slots { get; set; } = [];
    public string? Error { get; set; }
}
