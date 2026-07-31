namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IGroundRepository
{
    Task<IReadOnlyList<Ground>> GetAllAsync(GroundListRequest request, CancellationToken ct = default);
    Task<Ground?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Ground>> FindByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task CreateAsync(Ground ground, CancellationToken ct = default);
    Task UpdateAsync(Ground ground, CancellationToken ct = default);
    Task DeleteAsync(Ground ground, CancellationToken ct = default);
}