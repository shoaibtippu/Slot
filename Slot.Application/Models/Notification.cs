namespace Slot.Application.Models;

public class Notification : FullyAuditedEntity<Guid>
{
    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    #region Relationships

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    #endregion
}