namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IFavoriteRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid groundId, CancellationToken ct = default);
    Task CreateAsync(FavoriteGround favoriteGround, CancellationToken ct = default);
    Task DeleteAsync(FavoriteGround favoriteGround, CancellationToken ct = default);
    Task<FavoriteGround?> FindAsync(Guid userId, Guid groundId, CancellationToken ct = default);
    Task<IReadOnlyList<Ground>> GetMyFavoriteGroundsAsync(Guid userId, CancellationToken ct = default);
}
