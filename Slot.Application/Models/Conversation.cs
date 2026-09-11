namespace Slot.Application.Models;

public class Conversation : FullyAuditedEntity<Guid>
{
    public Guid? BookingId { get; set; }

    public Guid GroundOwnerId { get; set; }
    public User? GroundOwner { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Message> Messages { get; set; } = [];
}