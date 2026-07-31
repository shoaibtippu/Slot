namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetGrounds;

public class GetGroundsEndpointRequest
{
    public string? Search { get; set; }
    public Guid? SportId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? RadiusKm { get; set; }
    public decimal? MinHourlyRate { get; set; }
    public decimal? MaxHourlyRate { get; set; }
}
