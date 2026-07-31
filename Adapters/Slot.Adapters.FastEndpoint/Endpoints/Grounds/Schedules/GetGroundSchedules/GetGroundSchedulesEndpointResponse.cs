namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Schedules.GetGroundSchedules;

public class GetGroundSchedulesEndpointResponse
{
    public IReadOnlyList<GroundScheduleResponse> Schedules { get; set; } = [];
    public string? Error { get; set; }
}
