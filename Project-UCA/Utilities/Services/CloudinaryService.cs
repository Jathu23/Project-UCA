using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Project_UCA.Utilities.Interface;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Project_UCA.Middleware;

namespace Project_UCA.Utilities.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _uploadFolder;
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1MB
        private readonly string[] AllowedImageTypes = { "image/png", "image/jpeg" };

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];
            _uploadFolder = config["Cloudinary:UploadFolder"] ?? "Project_UCA/Signatures";
            _secretKey = config["TempUrl:SecretKey"] ?? "your-secret-key-for-temp-urls";

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            _httpClient = new HttpClient();
        }

        public async Task<(string TempUrl, string OriginalUrl, string PublicId)> UploadImageAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new BadHttpRequestException("No file provided.");
            if (file.Length > MaxFileSizeBytes)
                throw new BadHttpRequestException("File too large.");
            if (!AllowedImageTypes.Contains(file.ContentType))
                throw new BadHttpRequestException("Unsupported file type.");

            using var stream = file.OpenReadStream();
            var publicId = $"{_uploadFolder}/{fileName}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Overwrite = true,
            
            };


            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

            var originalUrl = result.SecureUrl.ToString();
            var tempUrl = GenerateStatelessTempUrl(publicId, 300); // 5 minutes expiry

            return (tempUrl, originalUrl, publicId);
        }

        public Task<string> GetImageAsync(string publicId)
        {
            var tempUrl = GenerateStatelessTempUrl(publicId, 300); // 5 minutes expiry
            return Task.FromResult(tempUrl);
        }
        //public async Task<string> GetImageAsync(string publicId)
        //{
        //    var url = _cloudinary.Api.UrlImgUp
        //        .Signed(true)
        //        .Secure(true)
        //        .BuildUrl(publicId);

        //    return url;
        //}

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Type = "upload"
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Result != "ok")
                throw new Exception($"Failed to delete image: {result.Error?.Message}");
        }

        public async Task<byte[]> DownloadImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("Public ID is required.", nameof(publicId));

           
            var originalUrl = _cloudinary.Api.UrlImgUp
              .Signed(true)
              .Secure(true)
              .BuildUrl(publicId);

            var getResourceParams = new GetResourceParams($"Project_UCA/Signatures/{publicId}")
            {
                ResourceType = ResourceType.Image
            };
            var resource = await _cloudinary.GetResourceAsync(getResourceParams);

            try
            {
                var response = await _httpClient.GetAsync(originalUrl);
                response.EnsureSuccessStatusCode(); // Throws if not 2xx status
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to download image from Cloudinary: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error downloading image: {ex.Message}", ex);
            }
        }
        //public async Task<string> UploadImageAsync(IFormFile file, string publicId)
        //{
        //    if (file == null || file.Length == 0)
        //        throw new BadRequestException("No file provided.");

        //    if (file.Length > MaxFileSizeBytes)
        //        throw new BadRequestException("File size exceeds 1MB limit.");

        //    if (!AllowedImageTypes.Contains(file.ContentType))
        //        throw new BadRequestException("Only PNG and JPEG images are allowed.");

        //    using var stream = file.OpenReadStream();
        //    var uploadParams = new ImageUploadParams
        //    {
        //        File = new FileDescription(file.FileName, stream),
        //        PublicId = $"Project_UCA/Signatures/{publicId}",
        //        Overwrite = true,
        //        Type = "authenticated" // ✅ Authenticated upload
        //    };

        //    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        //    if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        //        throw new BadRequestException("Failed to upload image to Cloudinary.");

        //    // ✅ Generate signed URL
        //    var signedUrl = _cloudinary.Api.UrlImgUp
        //        .Signed(true)
        //        .Type("authenticated")
        //        .Secure(true)
        //        .PublicId(uploadResult.PublicId)
        //        .BuildUrl();

        //    return signedUrl;
        //}

        private string GenerateStatelessTempUrl(string publicId, int expireSeconds = 300)
        {
            var expiryTime = DateTimeOffset.UtcNow.AddSeconds(expireSeconds).ToUnixTimeSeconds();

            var urlData = new TempUrlData
            {
                PublicId = publicId,
                ExpiryTime = expiryTime,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var jsonData = JsonSerializer.Serialize(urlData);
            var signature = CreateSignature(jsonData);

            var signedData = new SignedTempUrlData
            {
                Data = urlData,
                Signature = signature
            };

            var signedJsonData = JsonSerializer.Serialize(signedData);
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedJsonData))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            return $"/api/cloudinary/secure-image/{encodedData}";
        }

        public (bool IsValid, string PublicId) ValidateTempUrl(string encodedToken)
        {
            try
            {
                var paddedToken = encodedToken.Replace('-', '+').Replace('_', '/');
                while (paddedToken.Length % 4 != 0)
                    paddedToken += "=";

                var decodedBytes = Convert.FromBase64String(paddedToken);
                var jsonData = Encoding.UTF8.GetString(decodedBytes);

                var signedData = JsonSerializer.Deserialize<SignedTempUrlData>(jsonData);
                if (signedData?.Data == null || string.IsNullOrEmpty(signedData.Signature))
                    return (false, string.Empty);

                var originalJson = JsonSerializer.Serialize(signedData.Data);
                var expectedSignature = CreateSignature(originalJson);
                if (signedData.Signature != expectedSignature)
                    return (false, string.Empty);

                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentTime > signedData.Data.ExpiryTime)
                    return (false, string.Empty);

                return (true, signedData.Data.PublicId);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        private string CreateSignature(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }

    public class TempUrlData
    {
        public string PublicId { get; set; } = string.Empty;
        public long ExpiryTime { get; set; }
        public long Timestamp { get; set; }
    }

    public class SignedTempUrlData
    {
        public TempUrlData Data { get; set; } = new();
        public string Signature { get; set; } = string.Empty;
    }
}
