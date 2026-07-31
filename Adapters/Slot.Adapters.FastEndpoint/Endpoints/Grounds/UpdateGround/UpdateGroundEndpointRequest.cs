namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.UpdateGround;

public class UpdateGroundEndpointRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? AlternatePhoneNumber { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal AdvancePercentage { get; set; }
}
