namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CreateBooking;

public class CreateBookingEndpointResponse
{
    public bool Success { get; set; }
    public BookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
