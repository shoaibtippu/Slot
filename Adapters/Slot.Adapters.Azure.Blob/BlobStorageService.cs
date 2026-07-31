namespace Slot.Adapters.Azure.Blob;

public class BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration) : IBlobStorageService
{
    private readonly string _containerName = configuration["BlobStorage:ContainerName"] ?? throw new InvalidOperationException("BlobStorage:ContainerName is required.");

    public async Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken ct = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri))
            return;

        var path = uri.AbsolutePath.TrimStart('/');
        var containerName = _containerName.Trim('/');
        if (!path.StartsWith(containerName + "/", StringComparison.OrdinalIgnoreCase))
            return;

        var blobName = path[(containerName.Length + 1)..];
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
    }
}