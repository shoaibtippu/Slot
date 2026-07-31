namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CompleteBooking;

public class CompleteBookingEndpointResponse
{
    public bool Success { get; set; }
    public BookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
