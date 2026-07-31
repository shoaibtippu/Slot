namespace Slot.Application.Services;

public class GroundService(IGroundRepository groundRepository, IUserRepository userRepository) : IGroundService
{
    public async Task<Result<IReadOnlyList<GroundListItemResponse>>> GetAllAsync(GroundListRequest request, CancellationToken ct = default)
    {
        var grounds = await groundRepository.GetAllAsync(request, ct);
        return Result.Success<IReadOnlyList<GroundListItemResponse>>(grounds.Select(MapListItem).ToList());
    }

    public async Task<Result<GroundDetailResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ground = await groundRepository.FindByIdAsync(id, ct);
        return ground is null
            ? Result.Failure<GroundDetailResponse>(Error.NotFound("Ground not found."))
            : Result.Success(MapDetail(ground));
    }

    public async Task<Result<IReadOnlyList<GroundListItemResponse>>> GetMyGroundsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<GroundListItemResponse>>(Error.NotFound("User not found."));

        var grounds = await groundRepository.FindByOwnerIdAsync(profile.Id, ct);
        return Result.Success<IReadOnlyList<GroundListItemResponse>>(grounds.Select(MapListItem).ToList());
    }

    public async Task<Result<GroundDetailResponse>> CreateAsync(string userIdentityId, CreateGroundRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<GroundDetailResponse>(Error.NotFound("User not found."));

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result.Failure<GroundDetailResponse>(Error.Validation("Phone number is required."));

        var ground = new Ground
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PhoneNumber = request.PhoneNumber.Trim(),
            AlternatePhoneNumber = string.IsNullOrWhiteSpace(request.AlternatePhoneNumber) ? null : request.AlternatePhoneNumber.Trim(),
            HourlyRate = request.HourlyRate,
            AdvancePercentage = request.AdvancePercentage,
            AverageRating = 0,
            TotalReviews = 0,
            OwnerId = profile.Id
        };

        await groundRepository.CreateAsync(ground, ct);

        var created = await groundRepository.FindByIdAsync(ground.Id, ct);
        return created is null
            ? Result.Failure<GroundDetailResponse>(Error.NotFound("Ground not found."))
            : Result.Success(MapDetail(created));
    }

    public async Task<Result<GroundDetailResponse>> UpdateAsync(string userIdentityId, Guid id, UpdateGroundRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<GroundDetailResponse>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(id, ct);
        if (ground is null)
            return Result.Failure<GroundDetailResponse>(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure<GroundDetailResponse>(Error.UnAuthorized("You are not allowed to modify this ground."));

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result.Failure<GroundDetailResponse>(Error.Validation("Phone number is required."));

        ground.Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
        ground.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        ground.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        ground.Latitude = request.Latitude;
        ground.Longitude = request.Longitude;
        ground.PhoneNumber = request.PhoneNumber.Trim();
        ground.AlternatePhoneNumber = string.IsNullOrWhiteSpace(request.AlternatePhoneNumber) ? null : request.AlternatePhoneNumber.Trim();
        ground.HourlyRate = request.HourlyRate;
        ground.AdvancePercentage = request.AdvancePercentage;

        await groundRepository.UpdateAsync(ground, ct);

        var updated = await groundRepository.FindByIdAsync(ground.Id, ct);
        return updated is null
            ? Result.Failure<GroundDetailResponse>(Error.NotFound("Ground not found."))
            : Result.Success(MapDetail(updated));
    }

    public async Task<Result> DeleteAsync(string userIdentityId, Guid id, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(id, ct);
        if (ground is null)
            return Result.Failure(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure(Error.UnAuthorized("You are not allowed to modify this ground."));

        await groundRepository.DeleteAsync(ground, ct);
        return Result.Success();
    }

    private static GroundListItemResponse MapListItem(Ground ground)
    {
        var coverImage = ground.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
        var sports = ground.Sports.Select(s => s.Sport.Name).Distinct().ToList();
        return new GroundListItemResponse(ground.Id, ground.Name, ground.Address, ground.Latitude, ground.Longitude, ground.HourlyRate, ground.AverageRating, ground.TotalReviews, coverImage, sports);
    }

    private static GroundDetailResponse MapDetail(Ground ground)
    {
        var coverImage = ground.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
        return new GroundDetailResponse(
            ground.Id,
            ground.Name,
            ground.Description,
            ground.Address,
            ground.Latitude,
            ground.Longitude,
            ground.PhoneNumber,
            ground.AlternatePhoneNumber,
            ground.HourlyRate,
            ground.AdvancePercentage,
            ground.AverageRating,
            ground.TotalReviews,
            coverImage,
            ground.OwnerId,
            ground.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new GroundImageResponse(i.Id, i.ImageUrl, i.DisplayOrder))
                .ToList(),
            ground.Sports
                .Select(gs => new GroundSportResponse(gs.SportId, gs.Sport.Name, gs.Sport.IconUrl))
                .ToList(),
            ground.Schedules
                .OrderBy(s => s.DayOfWeek)
                .Select(s => new GroundScheduleResponse(s.DayOfWeek, s.OpeningTime, s.ClosingTime, s.IsClosed))
                .ToList());
    }
}
