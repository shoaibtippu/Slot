namespace Slot.Adapters.PostgreSql;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgreSqlAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Default")));

        // Persistence repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPushNotificationTokenRepository, PushNotificationTokenRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ISportRepository, SportRepository>();
        services.AddScoped<IGroundRepository, GroundRepository>();

        return services;
    }
}