namespace Slot.Adapters.FastEndpoint.Endpoints.Bookings.CreateBooking;

public class CreateBookingEndpointRequest
{
    public Guid GroundId { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Notes { get; set; }
}
