namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.UpdateBookingStatus;

public class UpdateBookingStatusEndpointResponse
{
    public bool Success { get; set; }
    public BookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
