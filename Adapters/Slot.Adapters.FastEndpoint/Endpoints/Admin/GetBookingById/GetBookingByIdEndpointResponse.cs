namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetBookingById;

public class GetBookingByIdEndpointResponse
{
    public AdminBookingDetailResponse? Booking { get; set; }
    public string? Error { get; set; }
}
