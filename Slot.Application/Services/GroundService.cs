namespace Slot.Application.Services;

public class GroundService(IGroundRepository groundRepository, IUserRepository userRepository, IBlobStorageService blobStorageService, IBookingRepository bookingRepository, IReviewService reviewService) : IGroundService
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

    public async Task<Result<IReadOnlyList<GroundImageResponse>>> GetImagesAsync(Guid groundId, CancellationToken ct = default)
    {
        var images = await groundRepository.GetImagesAsync(groundId, ct);
        return Result.Success<IReadOnlyList<GroundImageResponse>>(images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new GroundImageResponse(i.Id, i.ImageUrl, i.DisplayOrder))
            .ToList());
    }

    public async Task<Result<IReadOnlyList<GroundImageResponse>>> UploadImagesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundImageUploadRequest> images, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.UnAuthorized("You are not allowed to modify this ground."));

        if (images.Count == 0)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.Validation("At least one image is required."));

        var existingImages = await groundRepository.GetImagesAsync(groundId, ct);
        var nextDisplayOrder = existingImages.Count == 0 ? 1 : existingImages.Max(i => i.DisplayOrder) + 1;

        var createdImages = new List<GroundImage>();
        foreach (var image in images)
        {
            var extension = Path.GetExtension(image.FileName);
            var blobName = $"{userIdentityId}/groundimages/ground/{groundId}/{Guid.NewGuid()}{extension}";
            var imageUrl = await blobStorageService.UploadAsync(image.Content, blobName, image.ContentType, ct);

            createdImages.Add(new GroundImage
            {
                Id = Guid.NewGuid(),
                GroundId = groundId,
                ImageUrl = imageUrl,
                DisplayOrder = nextDisplayOrder++
            });
        }

        await groundRepository.AddImagesAsync(createdImages, ct);

        return Result.Success<IReadOnlyList<GroundImageResponse>>(createdImages
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new GroundImageResponse(i.Id, i.ImageUrl, i.DisplayOrder))
            .ToList());
    }

    public async Task<Result<IReadOnlyList<GroundImageResponse>>> ReorderImagesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundImageOrderRequest> images, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.UnAuthorized("You are not allowed to modify this ground."));

        var currentImages = await groundRepository.GetImagesAsync(groundId, ct);
        var imageMap = currentImages.ToDictionary(i => i.Id);

        foreach (var order in images)
        {
            if (imageMap.TryGetValue(order.ImageId, out var image))
                image.DisplayOrder = order.DisplayOrder;
            else
                return Result.Failure<IReadOnlyList<GroundImageResponse>>(Error.NotFound("One or more images were not found."));
        }

        await groundRepository.UpdateImagesAsync(imageMap.Values, ct);

        return Result.Success<IReadOnlyList<GroundImageResponse>>(imageMap.Values
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new GroundImageResponse(i.Id, i.ImageUrl, i.DisplayOrder))
            .ToList());
    }

    public async Task<Result> DeleteImageAsync(string userIdentityId, Guid groundId, Guid imageId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure(Error.UnAuthorized("You are not allowed to modify this ground."));

        var image = await groundRepository.FindImageAsync(groundId, imageId, ct);
        if (image is null)
            return Result.Failure(Error.NotFound("Ground image not found."));

        await blobStorageService.DeleteAsync(image.ImageUrl, ct);
        await groundRepository.DeleteImageAsync(image, ct);

        var remaining = await groundRepository.GetImagesAsync(groundId, ct);
        var ordered = remaining.OrderBy(i => i.DisplayOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].DisplayOrder = i + 1;

        await groundRepository.UpdateImagesAsync(ordered, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GroundScheduleResponse>>> GetSchedulesAsync(Guid groundId, CancellationToken ct = default)
    {
        var schedules = await groundRepository.GetSchedulesAsync(groundId, ct);
        return Result.Success<IReadOnlyList<GroundScheduleResponse>>(schedules
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new GroundScheduleResponse(s.DayOfWeek, s.OpeningTime, s.ClosingTime, s.IsClosed))
            .ToList());
    }

    public async Task<Result<IReadOnlyList<GroundScheduleResponse>>> ReplaceSchedulesAsync(string userIdentityId, Guid groundId, IReadOnlyList<GroundScheduleRequest> schedules, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<GroundScheduleResponse>>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<IReadOnlyList<GroundScheduleResponse>>(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure<IReadOnlyList<GroundScheduleResponse>>(Error.UnAuthorized("You are not allowed to modify this ground."));

        if (schedules.Count == 0)
            return Result.Failure<IReadOnlyList<GroundScheduleResponse>>(Error.Validation("At least one schedule is required."));

        var entities = schedules.Select(s => new GroundSchedule
        {
            Id = Guid.NewGuid(),
            GroundId = groundId,
            DayOfWeek = s.DayOfWeek,
            OpeningTime = s.OpeningTime,
            ClosingTime = s.ClosingTime,
            IsClosed = s.IsClosed
        }).ToList();

        await groundRepository.ReplaceSchedulesAsync(groundId, entities, ct);

        return Result.Success<IReadOnlyList<GroundScheduleResponse>>(entities
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new GroundScheduleResponse(s.DayOfWeek, s.OpeningTime, s.ClosingTime, s.IsClosed))
            .ToList());
    }

    public async Task<Result<IReadOnlyList<GroundAvailabilityBlockResponse>>> BlockAvailabilityAsync(string userIdentityId, Guid groundId, GroundAvailabilityBlockRequest request, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<IReadOnlyList<GroundAvailabilityBlockResponse>>(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<IReadOnlyList<GroundAvailabilityBlockResponse>>(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure<IReadOnlyList<GroundAvailabilityBlockResponse>>(Error.UnAuthorized("You are not allowed to modify this ground."));

        if (request.EndTime <= request.StartTime)
            return Result.Failure<IReadOnlyList<GroundAvailabilityBlockResponse>>(Error.Validation("End time must be greater than start time."));

        var block = new GroundAvailability
        {
            Id = Guid.NewGuid(),
            GroundId = groundId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsBlocked = true
        };

        await groundRepository.AddAvailabilityBlockAsync(block, ct);

        return Result.Success<IReadOnlyList<GroundAvailabilityBlockResponse>>(new[]
        {
            new GroundAvailabilityBlockResponse(block.Id, block.Date, block.StartTime, block.EndTime, block.IsBlocked)
        });
    }

    public async Task<Result> UnblockAvailabilityAsync(string userIdentityId, Guid groundId, Guid blockId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure(Error.NotFound("User not found."));

        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure(Error.NotFound("Ground not found."));

        if (ground.OwnerId != profile.Id)
            return Result.Failure(Error.UnAuthorized("You are not allowed to modify this ground."));

        var block = await groundRepository.FindAvailabilityBlockAsync(groundId, blockId, ct);
        if (block is null)
            return Result.Failure(Error.NotFound("Blocked slot not found."));

        await groundRepository.DeleteAvailabilityBlockAsync(block, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GroundAvailabilitySlotResponse>>> GetAvailabilityAsync(Guid groundId, DateOnly date, CancellationToken ct = default)
    {
        var ground = await groundRepository.FindByIdAsync(groundId, ct);
        if (ground is null)
            return Result.Failure<IReadOnlyList<GroundAvailabilitySlotResponse>>(Error.NotFound("Ground not found."));

        var schedules = await groundRepository.GetSchedulesAsync(groundId, ct);
        var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek && !s.IsClosed);
        if (schedule is null)
            return Result.Success<IReadOnlyList<GroundAvailabilitySlotResponse>>(Array.Empty<GroundAvailabilitySlotResponse>());

        var blocks = await groundRepository.GetAvailabilityBlocksAsync(groundId, date, ct);
        var bookings = await groundRepository.GetBookingsAsync(groundId, date, ct);

        var slotLength = TimeSpan.FromMinutes(60);
        var result = new List<GroundAvailabilitySlotResponse>();
        for (var start = schedule.OpeningTime; start < schedule.ClosingTime; start = start.Add(slotLength))
        {
            var end = start.Add(slotLength);
            if (end > schedule.ClosingTime)
                end = schedule.ClosingTime;

            var availability = blocks.FirstOrDefault(b => Overlaps(start, end, b.StartTime, b.EndTime));
            if (availability is not null)
            {
                result.Add(new GroundAvailabilitySlotResponse(start, end, "Blocked", null, availability.Id));
                continue;
            }

            var booking = bookings.FirstOrDefault(b => Overlaps(start, end, b.StartTime, b.EndTime));
            if (booking is not null)
            {
                result.Add(new GroundAvailabilitySlotResponse(start, end, "Booked", booking.Id, null));
                continue;
            }

            result.Add(new GroundAvailabilitySlotResponse(start, end, "Available", null, null));
        }

        return Result.Success<IReadOnlyList<GroundAvailabilitySlotResponse>>(result);
    }

    public async Task<Result<OwnerStatsResponse>> GetOwnerStatsAsync(string userIdentityId, CancellationToken ct = default)
    {
        var (identity, profile) = await userRepository.FindByIdentityIdAsync(userIdentityId, ct);
        if (identity is null || profile is null)
            return Result.Failure<OwnerStatsResponse>(Error.NotFound("User not found."));

        var grounds = await groundRepository.FindByOwnerIdAsync(profile.Id, ct);
        var allBookings = await bookingRepository.GetGroundOwnerBookingsAsync(profile.Id, ct);

        var totalRevenue = allBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalAmount);

        var pendingRevenue = allBookings
            .Where(b => b.Status == BookingStatus.Approved)
            .Sum(b => b.TotalAmount);

        var avgRating = grounds.Count > 0 ? grounds.Average(g => (double)g.AverageRating) : 0.0;

        var latestReviews = new List<OwnerLatestReview>();
        foreach (var ground in grounds)
        {
            if (latestReviews.Count >= 3) break;
            var reviewsResult = await reviewService.GetByGroundAsync(ground.Id, new PagedSearchSortDto { PageNumber = 1, PageSize = 3 }, ct);
            if (!reviewsResult.IsSuccess || reviewsResult.Value is null) continue;
            foreach (var r in reviewsResult.Value.Data)
            {
                if (latestReviews.Count >= 3) break;
                latestReviews.Add(new OwnerLatestReview(r.UserEmail, r.Rating, r.Comment, r.GroundName, r.CreatedAt));
            }
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sevenDaysAgo = today.AddDays(-6);
        var countsByDate = allBookings
            .Where(b => b.BookingDate >= sevenDaysAgo && b.BookingDate <= today)
            .GroupBy(b => b.BookingDate)
            .ToDictionary(g => g.Key, g => g.Count());
        var weeklyBookings = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = sevenDaysAgo.AddDays(i);
                return new WeeklyBookingItem(
                    date.DayOfWeek.ToString()[..3],
                    countsByDate.TryGetValue(date, out var c) ? c : 0);
            })
            .ToList();

        return Result.Success(new OwnerStatsResponse(
            TotalGrounds: grounds.Count,
            TotalBookings: allBookings.Count,
            PendingBookings: allBookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings: allBookings.Count(b => b.Status == BookingStatus.Approved),
            CompletedBookings: allBookings.Count(b => b.Status == BookingStatus.Completed),
            CancelledBookings: allBookings.Count(b => b.Status is BookingStatus.Cancelled or BookingStatus.Rejected),
            TotalRevenue: totalRevenue,
            PendingRevenue: pendingRevenue,
            AverageRating: (decimal)avgRating,
            LatestReviews: latestReviews,
            WeeklyBookings: weeklyBookings));
    }

    private static bool Overlaps(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        => start1 < end2 && start2 < end1;

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
