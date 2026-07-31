namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CancelBooking;

public class CancelBookingEndpointResponse
{
    public bool Success { get; set; }
    public BookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
