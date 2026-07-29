namespace Slot.Application.Models;

public class GroundTransaction : FullyAuditedEntity<Guid>
{
    public decimal Amount { get; set; }

    public decimal CommissionAmount { get; set; }

    public decimal OwnerAmount { get; set; }

    #region Relationships

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    #endregion
}