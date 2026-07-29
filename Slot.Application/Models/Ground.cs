namespace Slot.Application.Models;

public class Ground : FullyAuditedEntity<Guid>
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? AlternatePhoneNumber { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal AdvancePercentage { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }

    #region RelationShips

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<GroundImage> Images { get; set; } = [];
    public ICollection<GroundSport> Sports { get; set; } = [];
    public ICollection<GroundSchedule> Schedules { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];

    #endregion
}