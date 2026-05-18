using Application.Common.Settings;
using Application.Interfaces.Services.Media;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class BlobService : IBlobService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly BlobContainerClient _containerClient;

    public BlobService(IOptions<BlobSettings> options)
    {
        var settings = options.Value;

        _containerClient = new BlobContainerClient(
            settings.ConnectionString,
            settings.ContainerName
        );
    }

    public async Task<string> UploadImageAsync(IFormFile file, string? oldImageUrl = null)
    {
        ValidateFile(file);

        if (!string.IsNullOrWhiteSpace(oldImageUrl))
        {
            await DeleteImageAsync(oldImageUrl);
        }

        var extension = Path.GetExtension(file.FileName).ToLower();

        var fileName = $"{Guid.NewGuid()}{extension}";

        var blobClient = _containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        try
        {
            var uri = new Uri(imageUrl);

            var fileName = Path.GetFileName(uri.LocalPath);

            var blobClient = _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
        catch { }
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new Exception("File is empty");
        }

        if (file.Length > MaxFileSize)
        {
            throw new Exception("File size exceeded maximum limit of 5MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new Exception("Only jpg, jpeg, png, webp files are allowed");
        }
    }
}
