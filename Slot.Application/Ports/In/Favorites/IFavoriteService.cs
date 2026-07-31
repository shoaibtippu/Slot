namespace Slot.Application.Ports.In.Favorites;

public interface IFavoriteService
{
    Task<Result> AddAsync(string userIdentityId, Guid groundId, CancellationToken ct = default);
    Task<Result> RemoveAsync(string userIdentityId, Guid groundId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<FavoriteGroundResponse>>> GetMyFavoritesAsync(string userIdentityId, CancellationToken ct = default);
}

public record FavoriteGroundResponse(Guid GroundId, string? Name, string? Address, decimal HourlyRate, decimal AverageRating, string? CoverImageUrl);
