namespace Slot.Application.Models;

public class GroundSport
{
    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;

    public Guid SportId { get; set; }
    public Sport Sport { get; set; } = null!;
}