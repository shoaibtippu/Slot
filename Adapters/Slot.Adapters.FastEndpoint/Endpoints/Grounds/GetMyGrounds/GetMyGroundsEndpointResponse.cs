namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.GetMyGrounds;

public class GetMyGroundsEndpointResponse
{
    public IReadOnlyList<GroundListItemResponse> Grounds { get; set; } = [];
    public string? Error { get; set; }
}
