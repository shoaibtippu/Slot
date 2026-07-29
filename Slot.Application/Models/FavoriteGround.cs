namespace Slot.Application.Models;

public class FavoriteGround
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid GroundId { get; set; }
    public Ground Ground { get; set; } = null!;
}