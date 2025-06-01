namespace Project_UCA.Utilities.Interface
{
    public interface ICloudinaryService
    {
        Task<(string TempUrl, string OriginalUrl, string PublicId)> UploadImageAsync(IFormFile file, string fileName);
        Task<string> GetImageAsync(string publicId);
        Task DeleteImageAsync(string publicId);
        Task<byte[]> DownloadImageAsync(string publicId);
        (bool IsValid, string PublicId) ValidateTempUrl(string encodedToken);
    }
}