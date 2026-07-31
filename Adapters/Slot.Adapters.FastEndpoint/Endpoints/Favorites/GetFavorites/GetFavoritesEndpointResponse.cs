namespace Slot.Adapters.FastEndpoint.Endpoints.Favorites.GetFavorites;

public class GetFavoritesEndpointResponse
{
    public IReadOnlyList<FavoriteGroundResponse> Favorites { get; set; } = [];
    public string? Error { get; set; }
}
