namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetTransactions;

public class GetTransactionsEndpointResponse
{
    public IReadOnlyList<AdminTransactionListItemResponse> Transactions { get; set; } = [];
    public string? Error { get; set; }
}
