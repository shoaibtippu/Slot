namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetGroundOwnerBookings;

public class GetGroundOwnerBookingsEndpointResponse
{
    public IReadOnlyList<BookingListItemResponse> Bookings { get; set; } = [];
    public string? Error { get; set; }
}