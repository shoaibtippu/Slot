namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetGroundOwnerBookingsAsync(Guid ownerId, CancellationToken ct = default);
    Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Booking?> FindByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByGroundAndDateAsync(Guid groundId, DateOnly date, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task CreateAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
    Task DeleteAsync(Booking booking, CancellationToken ct = default);
}
