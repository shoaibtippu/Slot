using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Slot.Adapters.FastEndpoint.Endpoints.Favorites.GetFavorites;

[Authorize]
public class GetFavorites(IFavoriteService favoriteService) : EndpointWithoutRequest<GetFavoritesEndpointResponse>
{
    public override void Configure()
    {
        Get("/api/favorites");
        Summary(s =>
        {
            s.Summary = "Get my favorite grounds.";
            s.Response<GetFavoritesEndpointResponse>(200, "Favorites retrieved successfully.");
            s.Response(401, "Unauthorized.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var identityId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId))
        {
            await Send.ResponseAsync(new GetFavoritesEndpointResponse { Error = "Unauthorized." }, 401, ct);
            return;
        }

        var result = await favoriteService.GetMyFavoritesAsync(identityId, ct);
        if (!result.IsSuccess)
        {
            await Send.ResponseAsync(new GetFavoritesEndpointResponse { Error = result.Error!.Message }, result.Error.Code, ct);
            return;
        }

        await Send.OkAsync(new GetFavoritesEndpointResponse { Favorites = result.Value!, Error = null }, ct);
    }
}
