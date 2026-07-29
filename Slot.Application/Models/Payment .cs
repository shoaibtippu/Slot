namespace Slot.Application.Models;

public class Payment : FullyAuditedEntity<Guid>
{
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; }

    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }

    #region Relationships

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    #endregion
}