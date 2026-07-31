using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.GroundImages.UploadGroundImages;

[Authorize]
public class UploadGroundImages(IGroundService groundService) : Endpoint<UploadGroundImagesEndpointRequest, UploadGroundImagesEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/grounds/{id}/images");
        Summary(s =>
        {
            s.Summary = "Upload ground images to cloud storage.";
            s.Response<UploadGroundImagesEndpointResponse>(200, "Images uploaded successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
        });
    }

    public override async Task HandleAsync(UploadGroundImagesEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new UploadGroundImagesEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("id"), out var id))
        {
            await Send.ResponseAsync(new UploadGroundImagesEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var uploads = req.Images.Select(i => new GroundImageUploadRequest(i.OpenReadStream(), i.FileName, i.ContentType ?? "application/octet-stream")).ToList();
        var result = await groundService.UploadImagesAsync(identityId, id, uploads, ct);

        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new UploadGroundImagesEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new UploadGroundImagesEndpointResponse { Success = true, Images = result.Value, Error = null }, ct);
    }
}
