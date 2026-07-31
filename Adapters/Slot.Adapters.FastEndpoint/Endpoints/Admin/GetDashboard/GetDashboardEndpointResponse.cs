namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetDashboard;

public class GetDashboardEndpointResponse
{
    public AdminDashboardResponse? Dashboard { get; set; }
    public string? Error { get; set; }
}
