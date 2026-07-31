using Microsoft.AspNetCore.Authorization;

namespace Slot.Adapters.FastEndpoint.Endpoints.Sports.DeleteSport;

[Authorize(Roles = "Admin")]
public class DeleteSport(ISportService sportService) : EndpointWithoutRequest<DeleteSportEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/admin/sports/{id}");
        Summary(s =>
        {
            s.Summary = "Delete a sport.";
            s.Response<DeleteSportEndpointResponse>(200, "Sport deleted successfully.");
            s.Response(404, "Sport not found.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new DeleteSportEndpointResponse { Success = false, Error = "Invalid sport id." }, 400, ct);
            return;
        }

        var result = await sportService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteSportEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteSportEndpointResponse { Success = true, Error = null }, ct);
    }
}