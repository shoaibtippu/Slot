namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.CreateSport;

public class CreateSportEndpointResponse
{
    public bool Success { get; set; }
    public SportResponse? Sport { get; set; }
    public string? Error { get; set; }
}