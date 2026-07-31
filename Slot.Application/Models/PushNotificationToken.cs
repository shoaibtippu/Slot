namespace Slot.Application.Models;

public class PushNotificationToken : FullyAuditedEntity<Guid>
{
    public string Token { get; set; } = null!;
    public string? Platform { get; set; }

    #region Relationships

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    #endregion
}
