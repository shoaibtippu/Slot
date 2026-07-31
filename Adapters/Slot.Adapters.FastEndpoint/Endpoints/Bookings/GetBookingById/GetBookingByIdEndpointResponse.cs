namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.GetBookingById;

public class GetBookingByIdEndpointResponse
{
    public BookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
