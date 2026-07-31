using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.DeleteGround;

[Authorize]
public class DeleteGround(IGroundService groundService) : EndpointWithoutRequest<DeleteGroundEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/grounds/{id}");
        Summary(s =>
        {
            s.Summary = "Delete a ground.";
            s.Response<DeleteGroundEndpointResponse>(200, "Ground deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new DeleteGroundEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new DeleteGroundEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.DeleteAsync(identityId, id, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteGroundEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteGroundEndpointResponse { Success = true, Error = null }, ct);
    }
}
