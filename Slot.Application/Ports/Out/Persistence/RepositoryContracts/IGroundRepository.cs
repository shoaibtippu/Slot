namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IGroundRepository
{
    Task<IReadOnlyList<Ground>> GetAllAsync(GroundListRequest request, CancellationToken ct = default);
    Task<Ground?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Ground>> FindByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<GroundImage>> GetImagesAsync(Guid groundId, CancellationToken ct = default);
    Task<GroundImage?> FindImageAsync(Guid groundId, Guid imageId, CancellationToken ct = default);
    Task<IReadOnlyList<GroundSchedule>> GetSchedulesAsync(Guid groundId, CancellationToken ct = default);
    Task ReplaceSchedulesAsync(Guid groundId, IEnumerable<GroundSchedule> schedules, CancellationToken ct = default);
    Task<IReadOnlyList<GroundAvailability>> GetAvailabilityBlocksAsync(Guid groundId, DateOnly date, CancellationToken ct = default);
    Task<GroundAvailability?> FindAvailabilityBlockAsync(Guid groundId, Guid blockId, CancellationToken ct = default);
    Task AddAvailabilityBlockAsync(GroundAvailability block, CancellationToken ct = default);
    Task DeleteAvailabilityBlockAsync(GroundAvailability block, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetBookingsAsync(Guid groundId, DateOnly date, CancellationToken ct = default);
    Task CreateAsync(Ground ground, CancellationToken ct = default);
    Task UpdateAsync(Ground ground, CancellationToken ct = default);
    Task AddImagesAsync(IEnumerable<GroundImage> images, CancellationToken ct = default);
    Task UpdateImagesAsync(IEnumerable<GroundImage> images, CancellationToken ct = default);
    Task ReplaceImagesAsync(Guid groundId, IEnumerable<GroundImage> images, CancellationToken ct = default);
    Task DeleteAsync(Ground ground, CancellationToken ct = default);
    Task DeleteImageAsync(GroundImage image, CancellationToken ct = default);
}