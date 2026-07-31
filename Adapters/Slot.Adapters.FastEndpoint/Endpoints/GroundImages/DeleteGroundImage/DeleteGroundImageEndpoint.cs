using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.DeleteGroundImage;

[Authorize]
public class DeleteGroundImage(IGroundService groundService) : EndpointWithoutRequest<DeleteGroundImageEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/grounds/{id}/images/{imageId}");
        Summary(s =>
        {
            s.Summary = "Delete a ground image from cloud storage and the database.";
            s.Response<DeleteGroundImageEndpointResponse>(200, "Image deleted successfully.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground or image not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new DeleteGroundImageEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var groundId) || !Guid.TryParse(Route<string>("imageId"), out var imageId))
        {
            await Send.ResponseAsync(new DeleteGroundImageEndpointResponse { Success = false, Error = "Invalid id." }, 400, ct);
            return;
        }

        var result = await groundService.DeleteImageAsync(identityId, groundId, imageId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new DeleteGroundImageEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new DeleteGroundImageEndpointResponse { Success = true, Error = null }, ct);
    }
}
