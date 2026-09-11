namespace Slot.Application.Models;

public class ReviewReply : AuditedEntity<Guid>
{
    public required Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public required string Text { get; set; }
}
