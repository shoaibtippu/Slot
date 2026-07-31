namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetBookings;

public class GetBookingsEndpointResponse
{
    public IReadOnlyList<AdminBookingListItemResponse> Bookings { get; set; } = [];
    public string? Error { get; set; }
}
