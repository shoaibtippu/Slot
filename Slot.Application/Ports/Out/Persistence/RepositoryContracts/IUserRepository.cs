namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IUserRepository
{
    Task<(IdentityUser? identity, User? profile)> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<(IdentityUser? identity, User? profile)> FindByIdentityIdAsync(string identityId, CancellationToken ct = default);

    Task CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}