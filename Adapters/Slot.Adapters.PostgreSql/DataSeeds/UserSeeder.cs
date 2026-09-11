namespace Slot.Adapters.PostgreSql.DataSeeds;

public static class UserSeeder
{
    private const string UserEmail = "user@user.com";
    private const string UserPassword = "Hello@123";
    private const string UserRole = "User";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existing = await userManager.FindByEmailAsync(UserEmail);
        if (existing is not null)
        {
            logger.LogInformation("Test user '{Email}' already exists — skipping seed.", UserEmail);
            return;
        }

        var userIdentity = new IdentityUser
        {
            UserName = UserEmail,
            Email = UserEmail,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(userIdentity, UserPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to create test user Identity: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(userIdentity, UserRole);
        if (!roleResult.Succeeded)
        {
            logger.LogError("Failed to assign User role: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            return;
        }

        var userProfile = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            UserIdentityId = userIdentity.Id,
        };

        await db.Users.AddAsync(userProfile);
        await db.SaveChangesAsync();

        logger.LogInformation("Test user '{Email}' seeded successfully.", UserEmail);
    }
}
