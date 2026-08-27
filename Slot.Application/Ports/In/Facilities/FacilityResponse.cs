namespace Slot.Application.Ports.In.Facilities;

public record FacilityResponse(
    string Id,
    string? Name,
    string Status,
    string? ImageUrl,
    string? Location,
    decimal Rating,
    int TotalReviews,
    IReadOnlyList<string> Tags,
    int HourlyRate,
    string Currency,
    DateTime CreatedAt);