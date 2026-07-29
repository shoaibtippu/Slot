namespace Slot.Application.Models;

public class GroundImage : FullyAuditedEntity<Guid>
{
    public string ImageUrl { get; set; } = null!;

    public int DisplayOrder { get; set; }

    #region Relationships

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    #endregion
}