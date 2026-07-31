namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.GetSports;

public class GetSportsEndpointResponse
{
    public IReadOnlyList<SportResponse> Sports { get; set; } = [];
    public string? Error { get; set; }
}