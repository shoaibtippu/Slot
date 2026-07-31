using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetTransactions;

[Authorize(Roles = "Admin")]
public class GetTransactions(IAdminService adminService) : EndpointWithoutRequest<GetTransactionsEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/transactions");
        Summary(s =>
        {
            s.Summary = "Revenue report with aggregates.";
            s.Response<GetTransactionsEndpointResponse>(200, "Transactions retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await adminService.GetTransactionsAsync(ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetTransactionsEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetTransactionsEndpointResponse { Transactions = result.Value!, Error = null }, ct);
    }
}
