namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.CreateSport;

public class CreateSportEndpointRequest
{
    public string Name { get; set; } = null!;
    public string? IconUrl { get; set; }
}