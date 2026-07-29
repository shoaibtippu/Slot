namespace Slot.Application.Models;

public class Message : FullyAuditedEntity<Guid>
{
    public string Text { get; set; } = null!;

    public bool IsRead { get; set; }

    #region Relationships

    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    #endregion
}