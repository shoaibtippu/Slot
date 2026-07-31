namespace Slot.Application.Services;

public class FavoriteService(IFavoriteRepository favoriteRepository, IUserRepository userRepository, IGroundRepository groundRepository) : IFavoriteService
{
    public async Task<Result> AddAsync(string userIdentityId, Guid groundId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure(Error.NotFound("Ground not found."));

        if (await favoriteRepository.ExistsAsync(profile.Id, groundId, ct))
            return Result.Failure(Error.Conflict("Ground is already in favorites."));

        await favoriteRepository.CreateAsync(new FavoriteGround { UserId = profile.Id, GroundId = groundId }, ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(string userIdentityId, Guid groundId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var favorite = await favoriteRepository.FindAsync(profile.Id, groundId, ct);
        if (favorite is null)
            return Result.Failure(Error.NotFound("Favorite not found."));

        await favoriteRepository.DeleteAsync(favorite, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<FavoriteGroundResponse>>> GetMyFavoritesAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<FavoriteGroundResponse>>(Error.NotFound("User not found."));

        var grounds = await favoriteRepository.GetMyFavoriteGroundsAsync(profile.Id, ct);
        return Result.Success<IReadOnlyList<FavoriteGroundResponse>>(grounds.Select(Map).ToList());
    }

    private static FavoriteGroundResponse Map(Ground ground)
    {
        var coverImage = ground.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
        return new FavoriteGroundResponse(ground.Id, ground.Name, ground.Address, ground.HourlyRate, ground.AverageRating, coverImage);
    }
}
