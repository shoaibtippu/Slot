namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Schedules.UpdateGroundSchedules;

public class UpdateGroundSchedulesEndpointResponse
{
    public bool Success { get; set; }
    public IReadOnlyList<GroundScheduleResponse>? Schedules { get; set; }
    public string? Error { get; set; }
}
