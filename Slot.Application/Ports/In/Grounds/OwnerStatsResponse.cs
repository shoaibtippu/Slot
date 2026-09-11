namespace Slot.Application.Ports.In.Grounds;

public record OwnerStatsResponse(
    int TotalGrounds,
    int TotalBookings,
    int PendingBookings,
    int ConfirmedBookings,
    int CompletedBookings,
    int CancelledBookings,
    decimal TotalRevenue,
    decimal PendingRevenue,
    decimal AverageRating,
    IReadOnlyList<OwnerLatestReview> LatestReviews,
    IReadOnlyList<WeeklyBookingItem> WeeklyBookings);

public record OwnerLatestReview(
    string? ReviewerEmail,
    int Rating,
    string? Comment,
    string? GroundName,
    DateTime? CreatedAt);

public record WeeklyBookingItem(string Day, int Count);
