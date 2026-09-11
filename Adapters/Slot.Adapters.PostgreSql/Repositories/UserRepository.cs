namespace Slot.Adapters.PostgreSql.Repositories;

public class UserRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager) : IUserRepository
{
    public async Task<(IdentityUser? identity, User? profile)> FindByEmailAsync(
        string email, CancellationToken ct = default)
    {
        var identity = await userManager.FindByEmailAsync(email);

        if (identity is null)
            return (null, null);

        var profile = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserIdentityId == identity.Id, ct);
        
        return (identity, profile);
    }

    public async Task<(IdentityUser? identity, User? profile)> FindByIdentityIdAsync(
        string identityId, CancellationToken ct = default)
    {
        var identity = await userManager.FindByIdAsync(identityId);

        if (identity is null)
            return (null, null);

        var profile = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserIdentityId == identityId, ct);

        return (identity, profile);
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<(IdentityUser identity, User profile)>> SearchByEmailAsync(
        string emailQuery, int maxResults = 20, CancellationToken ct = default)
    {
        var matchedIdentities = userManager.Users
            .Where(u => u.Email != null && u.Email.Contains(emailQuery))
            .Take(maxResults)
            .ToList();

        if (matchedIdentities.Count == 0)
            return [];

        var ids = matchedIdentities.Select(u => u.Id).ToList();
        var profiles = await db.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.UserIdentityId))
            .ToListAsync(ct);

        return matchedIdentities
            .Select(identity => (identity, profiles.FirstOrDefault(p => p.UserIdentityId == identity.Id)))
            .Where(x => x.Item2 is not null)
            .Select(x => (x.identity, x.Item2!))
            .ToList();
    }

    public async Task<IReadOnlyList<(IdentityUser identity, User profile)>> SearchByNameAsync(
        string nameQuery, int maxResults = 20, CancellationToken ct = default)
    {
        var profiles = await db.Users
            .AsNoTracking()
            .Where(u => u.FullName != null && u.FullName.ToLower().Contains(nameQuery.ToLower()))
            .Take(maxResults)
            .ToListAsync(ct);

        if (profiles.Count == 0)
            return [];

        var identityIds = profiles.Select(p => p.UserIdentityId).ToList();
        var identities = userManager.Users
            .Where(u => identityIds.Contains(u.Id))
            .ToList();

        return profiles
            .Select(profile => (identities.FirstOrDefault(i => i.Id == profile.UserIdentityId), profile))
            .Where(x => x.Item1 is not null)
            .Select(x => (x.Item1!, x.profile))
            .ToList();
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }
}