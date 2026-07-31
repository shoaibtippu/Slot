namespace Slot.Adapters.FastEndpoint;

public static class DependencyInjection
{
    public static IServiceCollection AddFastEndpointAdapter(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISportService, SportService>();
        return services;
    }
}