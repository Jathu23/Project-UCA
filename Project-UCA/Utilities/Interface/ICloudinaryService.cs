using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Project_UCA.Utilities.Interface
{
    public interface ICloudinaryService
    {
        Task<(string TempUrl, string OriginalUrl, string PublicId)> UploadImageAsync(IFormFile file, string fileName);
        Task DeleteImageAsync(string publicId);
        Task<byte[]> DownloadImageAsync(string publicId);
        Task<string> GetImageById(string publicId);
        (bool IsValid, string PublicId) ValidateTempUrl(string token);
    }
}