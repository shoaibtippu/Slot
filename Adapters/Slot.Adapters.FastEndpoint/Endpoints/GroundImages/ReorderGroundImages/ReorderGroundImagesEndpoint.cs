using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.ReorderGroundImages;

[Authorize]
public class ReorderGroundImages(IGroundService groundService) : Endpoint<ReorderGroundImagesEndpointRequest, ReorderGroundImagesEndpointResponse>
{
    public override void Configure()
    {
        Put("/api/grounds/{id}/images/reorder");
        Summary(s =>
        {
            s.Summary = "Update display order of ground images.";
            s.Response<ReorderGroundImagesEndpointResponse>(200, "Images reordered successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(ReorderGroundImagesEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new ReorderGroundImagesEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new ReorderGroundImagesEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await groundService.ReorderImagesAsync(identityId, id, req.Images.Select(i => new GroundImageOrderRequest(i.ImageId, i.DisplayOrder)).ToList(), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new ReorderGroundImagesEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new ReorderGroundImagesEndpointResponse { Success = true, Images = result.Value, Error = null }, ct);
    }
}
