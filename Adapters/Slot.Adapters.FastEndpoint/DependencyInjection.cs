namespace Slot.Adapters.FastEndpoint;

public static class DependencyInjection
{
    public static IServiceCollection AddFastEndpointAdapter(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<ISportService, SportService>();
        services.AddScoped<IGroundService, GroundService>();
        return services;
    }
}