namespace Slot.Application.Ports.Out.Storage;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string blobUrl, CancellationToken ct = default);
}