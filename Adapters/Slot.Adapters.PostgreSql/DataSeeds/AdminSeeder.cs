namespace Slot.Adapters.PostgreSql.DataSeeds;

public static class AdminSeeder
{
    private const string AdminEmail = "admin@admin.com";
    private const string AdminPassword = "Hello@123";
    private const string AdminRole = "Admin";
    private const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // ── 1. Run any pending migrations ────────────────────────────
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");

        // ── 2. Seed roles ─────────────────────────────────────────────
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                    logger.LogInformation("Role '{Role}' created.", role);
                else
                    logger.LogError("Failed to create role '{Role}': {Errors}",
                        role, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // ── 3. Bail out if admin already exists ───────────────────────
        var existing = await userManager.FindByEmailAsync(AdminEmail);
        if (existing is not null)
        {
            logger.LogInformation("Admin '{Email}' already exists — skipping seed.", AdminEmail);
            return;
        }

        // ── 4. Create Identity user ───────────────────────────────────
        var adminIdentity = new IdentityUser
        {
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(adminIdentity, AdminPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to create admin Identity user: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        // ── 5. Assign Admin role ──────────────────────────────────────
        var roleResult = await userManager.AddToRoleAsync(adminIdentity, AdminRole);
        if (!roleResult.Succeeded)
        {
            logger.LogError("Failed to assign Admin role: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return;
        }

        // ── 6. Create the linked User profile ────────────────────────
        var adminProfile = new User
        {
            Id = Guid.NewGuid(),
            UserIdentityId = adminIdentity.Id,
        };

        await db.Users.AddAsync(adminProfile);
        await db.SaveChangesAsync();

        logger.LogInformation("Admin user '{Email}' seeded successfully.", AdminEmail);
    }
}