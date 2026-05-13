using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.Media
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(IFormFile file, string? oldImageUrl = null);

        Task DeleteImageAsync(string imageUrl);
    }
}
