namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Availability.BlockAvailability;

public class BlockAvailabilityEndpointRequest
{
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
