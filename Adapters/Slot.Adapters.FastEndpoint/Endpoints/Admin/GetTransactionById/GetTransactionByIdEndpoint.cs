using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Admin.GetTransactionById;

[Authorize(Roles = "Admin")]
public class GetTransactionById(IAdminService adminService) : EndpointWithoutRequest<GetTransactionByIdEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/admin/transactions/{id}");
        Summary(s =>
        {
            s.Summary = "Transaction detail (admin).";
            s.Response<GetTransactionByIdEndpointResponse>(200, "Transaction retrieved successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(403, "Forbidden.");
            s.Response(404, "Transaction not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new GetTransactionByIdEndpointResponse { Error = "Invalid transaction id." }, 400, ct);
            return;
        }

        var result = await adminService.GetTransactionByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetTransactionByIdEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetTransactionByIdEndpointResponse { Transaction = result.Value!, Error = null }, ct);
    }
}
