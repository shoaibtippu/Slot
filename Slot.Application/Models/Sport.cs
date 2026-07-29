namespace Slot.Application.Models;

public class Sport : FullyAuditedEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string? IconUrl { get; set; }

    #region Relationships

    public ICollection<GroundSport> GroundSports { get; set; } = [];

    #endregion
}