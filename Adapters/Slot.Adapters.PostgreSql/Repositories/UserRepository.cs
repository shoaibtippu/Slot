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

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }
}