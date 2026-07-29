namespace Slot.Application.Models;

public class GroundAvailability: FullyAuditedEntity<Guid>
{
    public Guid GroundId { get; set; }

    public DateOnly Date { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public bool IsBlocked { get; set; }
}