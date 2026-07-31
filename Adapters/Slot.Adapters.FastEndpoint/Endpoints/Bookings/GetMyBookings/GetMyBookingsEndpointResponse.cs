namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetMyBookings;

public class GetMyBookingsEndpointResponse
{
    public IReadOnlyList<BookingListItemResponse> Bookings { get; set; } = [];
    public string? Error { get; set; }
}
