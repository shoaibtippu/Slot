namespace Slot.Application.Models;

public class Booking : FullyAuditedEntity<Guid>
{
    public DateOnly BookingDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public decimal PricePerHour { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AdvanceAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public BookingStatus Status { get; set; }

    public string? Notes { get; set; }

    #region Relationships

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Payment> Payments { get; set; } = [];

    #endregion
}