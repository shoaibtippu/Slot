namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetTransactionById;

public class GetTransactionByIdEndpointResponse
{
    public AdminTransactionDetailResponse? Transaction { get; set; }
    public string? Error { get; set; }
}
