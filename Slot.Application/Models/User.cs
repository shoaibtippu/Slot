namespace Slot.Application.Models;

public class User : FullyAuditedEntity<Guid>
{
    public string? ImageUrl { get; set; }
    public string? FullName { get; set; }
    public string? City { get; set; }

    #region RelationShips

    public IdentityUser UserIdentity { get; set; } = null!;
    public string UserIdentityId { get; set; }

    public ICollection<Ground> OwnedGrounds { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];

    #endregion
}