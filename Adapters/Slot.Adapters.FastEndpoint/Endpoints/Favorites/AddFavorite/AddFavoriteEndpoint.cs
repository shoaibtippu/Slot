using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Favorites.AddFavorite;

[Authorize]
public class AddFavorite(IFavoriteService favoriteService) : EndpointWithoutRequest<AddFavoriteEndpointResponse>
{
    public override void Configure()
    {
        Post("/api/favorites/{groundId}");
        Summary(s =>
        {
            s.Summary = "Add ground to favorites.";
            s.Response<AddFavoriteEndpointResponse>(200, "Ground added to favorites.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Ground not found.");
            s.Response(409, "Ground is already in favorites.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new AddFavoriteEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("groundId"), out var groundId))
        {
            await Send.ResponseAsync(new AddFavoriteEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await favoriteService.AddAsync(identityId, groundId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new AddFavoriteEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new AddFavoriteEndpointResponse { Success = true, Error = null }, ct);
    }
}
