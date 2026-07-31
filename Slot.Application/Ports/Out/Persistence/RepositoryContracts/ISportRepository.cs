namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface ISportRepository
{
    Task<IReadOnlyList<Sport>> GetAllAsync(CancellationToken ct = default);
    Task<Sport?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task CreateAsync(Sport sport, CancellationToken ct = default);
    Task UpdateAsync(Sport sport, CancellationToken ct = default);
    Task DeleteAsync(Sport sport, CancellationToken ct = default);
}