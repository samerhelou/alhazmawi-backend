using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AlHazmawi.Infrastructure.Services;

/// <summary>
/// Local file storage implementation for development.
/// Files are stored in wwwroot/uploads.
/// In production, replace with Azure Blob Storage or S3.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalStorageService> _logger;
    
    // Allowed MIME types
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    
    // Max file size: 5MB
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    
    public LocalStorageService(IWebHostEnvironment env, ILogger<LocalStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }
    
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException($"File type '{contentType}' is not allowed.");
        
        if (fileStream.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File exceeds maximum size of 5MB.");
        
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        
        // Generate unique filename
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, uniqueName);
        
        using var fileStream2 = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStream2, cancellationToken);
        
        _logger.LogInformation("File uploaded: {FileName}", uniqueName);
        
        return $"/uploads/{uniqueName}";
    }
    
    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;
        
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FileName}", fileName);
        }
        
        return Task.CompletedTask;
    }
}
