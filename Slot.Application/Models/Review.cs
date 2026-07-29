namespace Slot.Application.Models;

public class Review : FullyAuditedEntity<Guid>
{
    public int Rating { get; set; }

    public string? Comment { get; set; }

    #region Relationships

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    #endregion
}