namespace AlHazmawi.Infrastructure.Services;

/// <summary>
/// Storage service abstraction. Swap LocalStorageService for Azure Blob / S3 in production.
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}
