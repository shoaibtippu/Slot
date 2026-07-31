using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Grounds.CreateGround;

[Authorize]
public class CreateGround(IGroundService groundService) : Endpoint<CreateGroundEndpointRequest, CreateGroundEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/grounds");
        Summary(s =>
        {
            s.Summary = "Create a ground.";
            s.Response<CreateGroundEndpointResponse>(200, "Ground created successfully.");
            s.Response(400, "Validation error.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CreateGroundEndpointRequest req, CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new CreateGroundEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await groundService.CreateAsync(identityId, new CreateGroundRequest(req.Name, req.Description, req.Address, req.Latitude, req.Longitude, req.PhoneNumber, req.AlternatePhoneNumber, req.HourlyRate, req.AdvancePercentage), ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new CreateGroundEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new CreateGroundEndpointResponse { Success = true, Ground = result.Value, Error = null }, ct);
    }
}
