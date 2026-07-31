namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetGrounds;

public class GetGroundsEndpointResponse
{
    public IReadOnlyList<AdminGroundListItemResponse> Grounds { get; set; } = [];
    public string? Error { get; set; }
}
