namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetGrounds;

public class GetGroundsEndpointResponse
{
    public IReadOnlyList<GroundListItemResponse> Grounds { get; set; } = [];
    public string? Error { get; set; }
}
