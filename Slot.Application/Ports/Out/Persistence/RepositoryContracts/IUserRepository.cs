namespace Slot.Application.Ports.Out.Persistence.RepositoryContracts;

public interface IUserRepository
{
    Task<(IdentityUser? identity, User? profile)> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<(IdentityUser? identity, User? profile)> FindByIdentityIdAsync(string identityId, CancellationToken ct = default);
    Task<IReadOnlyList<(IdentityUser identity, User profile)>> SearchByEmailAsync(string emailQuery, int maxResults = 20, CancellationToken ct = default);
    Task<IReadOnlyList<(IdentityUser identity, User profile)>> SearchByNameAsync(string nameQuery, int maxResults = 20, CancellationToken ct = default);

    Task CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}