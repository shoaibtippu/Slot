using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Favorites.RemoveFavorite;

[Authorize]
public class RemoveFavorite(IFavoriteService favoriteService) : EndpointWithoutRequest<RemoveFavoriteEndpointResponse>
{
    public override void Configure()
    {
        Delete("/api/favorites/{groundId}");
        Summary(s =>
        {
            s.Summary = "Remove ground from favorites.";
            s.Response<RemoveFavoriteEndpointResponse>(200, "Ground removed from favorites.");
            s.Response(401, "Unauthorized.");
            s.Response(404, "Favorite not found.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new RemoveFavoriteEndpointResponse { Success = false, Error = "Unauthorized." }, 401, ct);
            return;
        }

        if (!Guid.TryParse(Route<string>("groundId"), out var groundId))
        {
            await Send.ResponseAsync(new RemoveFavoriteEndpointResponse { Success = false, Error = "Invalid ground id." }, 400, ct);
            return;
        }

        var result = await favoriteService.RemoveAsync(identityId, groundId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new RemoveFavoriteEndpointResponse { Success = false, Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new RemoveFavoriteEndpointResponse { Success = true, Error = null }, ct);
    }
}
