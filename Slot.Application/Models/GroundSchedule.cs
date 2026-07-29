namespace Slot.Application.Models;

public class GroundSchedule : FullyAuditedEntity<Guid>
{
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan OpeningTime { get; set; }

    public TimeSpan ClosingTime { get; set; }

    public bool IsClosed { get; set; }

    #region Relationships

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    #endregion
}