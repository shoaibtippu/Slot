namespace Slot.Adapters.Azure.Blob;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureBlobAdapter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        return services;
    }
}