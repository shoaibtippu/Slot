namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.UpdateSport;

public class UpdateSportEndpointResponse
{
    public bool Success { get; set; }
    public SportResponse? Sport { get; set; }
    public string? Error { get; set; }
}