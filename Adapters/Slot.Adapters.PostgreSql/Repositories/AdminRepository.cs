namespace Slot.Adapters.PostgreSql.Repositories;

public class AdminRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : IAdminRepository
{
    public async Task<IReadOnlyList<(User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive)>> GetUsersAsync(CancellationToken ct = default)
    {
        var profiles = await db.Users
            .AsNoTracking()
            .Include(u => u.UserIdentity)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);

        var result = new List<(User, IdentityUser, IReadOnlyList<string>, bool)>();
        foreach (var profile in profiles)
        {
            var identity = profile.UserIdentity;
            var roles = await userManager.GetRolesAsync(identity);
            result.Add((profile, identity, roles.ToList(), identity.LockoutEnd is null || identity.LockoutEnd <= DateTimeOffset.UtcNow));
        }

        return result;
    }

    public async Task<(User Profile, IdentityUser Identity, IReadOnlyList<string> Roles, bool IsActive)?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await db.Users
            .AsNoTracking()
            .Include(u => u.UserIdentity)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (profile is null)
            return null;

        var roles = await userManager.GetRolesAsync(profile.UserIdentity);
        var isActive = profile.UserIdentity.LockoutEnd is null || profile.UserIdentity.LockoutEnd <= DateTimeOffset.UtcNow;
        return (profile, profile.UserIdentity, roles.ToList(), isActive);
    }

    public async Task UpdateUserRolesAsync(string identityUserId, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        var identity = await userManager.FindByIdAsync(identityUserId);
        if (identity is null)
            return;

        var currentRoles = await userManager.GetRolesAsync(identity);
        var removeResult = await userManager.RemoveFromRolesAsync(identity, currentRoles);
        if (!removeResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

        var addRoles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (addRoles.Length > 0)
        {
            var addResult = await userManager.AddToRolesAsync(identity, addRoles);
            if (!addResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }
    }

    public async Task DeactivateUserAsync(string identityUserId, CancellationToken ct = default)
    {
        var identity = await userManager.FindByIdAsync(identityUserId);
        if (identity is null)
            return;

        identity.LockoutEnabled = true;
        identity.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        await userManager.UpdateAsync(identity);
    }

    public async Task<IReadOnlyList<Ground>> GetGroundsAsync(CancellationToken ct = default)
    {
        return await db.Grounds
            .AsNoTracking()
            .Include(g => g.Owner)
                .ThenInclude(o => o.UserIdentity)
            .Include(g => g.Images)
            .Include(g => g.Bookings)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task DeleteGroundAsync(Ground ground, CancellationToken ct = default)
    {
        db.Grounds.Remove(ground);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsAsync(CancellationToken ct = default)
    {
        return await db.Bookings
            .AsNoTracking()
            .Include(b => b.Ground)
                .ThenInclude(g => g.Owner)
                    .ThenInclude(o => o.UserIdentity)
            .Include(b => b.User)
                .ThenInclude(u => u.UserIdentity)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsAsync(CancellationToken ct = default)
    {
        return await db.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Ground)
            .Include(p => p.Booking)
                .ThenInclude(b => b.User)
                    .ThenInclude(u => u.UserIdentity)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<int> GetUserCountAsync(CancellationToken ct = default) => db.Users.CountAsync(ct);
    public Task<int> GetGroundCountAsync(CancellationToken ct = default) => db.Grounds.CountAsync(ct);
    public Task<int> GetBookingCountAsync(CancellationToken ct = default) => db.Bookings.CountAsync(ct);

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default)
        => await db.Payments.Where(p => p.Status == PaymentStatus.Paid).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

    public async Task<decimal> GetPendingRevenueAsync(CancellationToken ct = default)
        => await db.Payments.Where(p => p.Status == PaymentStatus.Pending).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

    public async Task<decimal> GetCompletedRevenueAsync(CancellationToken ct = default)
        => await db.Payments.Where(p => p.Status == PaymentStatus.Paid).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
}
