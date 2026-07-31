namespace Slot.Adapters.PostgreSql;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgreSqlAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<IGroundRepository, GroundRepository>();

        return services;
    }
}