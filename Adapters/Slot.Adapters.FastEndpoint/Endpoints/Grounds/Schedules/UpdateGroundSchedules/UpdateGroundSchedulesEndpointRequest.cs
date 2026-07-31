namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.Schedules.UpdateGroundSchedules;

public class UpdateGroundSchedulesEndpointRequest
{
    public List<UpdateGroundScheduleItem> Schedules { get; set; } = [];
}

public class UpdateGroundScheduleItem
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsClosed { get; set; }
}
