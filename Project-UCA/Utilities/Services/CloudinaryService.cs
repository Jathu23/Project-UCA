using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Project_UCA.Middleware;
using Project_UCA.Utilities.Interface;
using System.Security.Cryptography;
using System.Text;


namespace Project_UCA.Utilities.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _uploadFolder;
        private readonly string _secretKey;
        private readonly IHttpClientFactory _httpClientFactory;
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1MB
        private readonly string[] AllowedImageTypes = { "image/png", "image/jpeg" };

        public CloudinaryService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];
            _uploadFolder = config["Cloudinary:UploadFolder"] ?? "Project_UCA/Signatures";
            _secretKey = config["TempUrl:SecretKey"] ?? throw new InvalidOperationException("Temp URL secret key is missing.");

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
                throw new InvalidOperationException("Cloudinary configuration is missing.");

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<(string TempUrl, string OriginalUrl, string PublicId)> UploadImageAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file provided.");
            if (file.Length > MaxFileSizeBytes)
                throw new BadRequestException("File size exceeds 1MB limit.");
            if (!AllowedImageTypes.Contains(file.ContentType))
                throw new BadRequestException("Only PNG and JPEG images are allowed.");
            if (string.IsNullOrWhiteSpace(fileName))
                throw new BadRequestException("File name is required.");

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
                throw new BadRequestException($"Cloudinary upload failed: {result.Error?.Message}");

            var originalUrl = result.SecureUrl.ToString();
            var tempUrl = GenerateTempUrl(publicId);

            return (tempUrl, originalUrl, publicId);
        }

        public async Task<string> GetImageById(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            return GenerateTempUrl(publicId);
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
               
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Result != "ok")
                throw new BadRequestException($"Failed to delete image: {result.Error?.Message}");
        }

        public async Task<byte[]> DownloadImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            var getResourceParams = new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            var resource = await _cloudinary.GetResourceAsync(getResourceParams);

            if (resource == null || resource.StatusCode != System.Net.HttpStatusCode.OK)
                throw new BadRequestException("Image not found.");

            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                var response = await httpClient.GetAsync(resource.SecureUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new BadRequestException($"Failed to download image: {ex.Message}");
            }
        }

        public (bool IsValid, string PublicId) ValidateTempUrl(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return (false, string.Empty);

            try
            {
                var decodedBytes = Convert.FromBase64String(token.PadRight(token.Length + (4 - token.Length % 4) % 4, '='));
                var parts = Encoding.UTF8.GetString(decodedBytes).Split(':');
                if (parts.Length != 3)
                    return (false, string.Empty);

                var publicId = parts[0];
                var expiry = long.Parse(parts[1]);
                var signature = parts[2];

                var data = $"{publicId}:{expiry}";
                var expectedSignature = CreateSignature(data);
                if (signature != expectedSignature)
                    return (false, string.Empty);

                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
                    return (false, string.Empty);

                return (true, publicId);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        private string GenerateTempUrl(string publicId)
        {
            var expiry = DateTimeOffset.UtcNow.AddSeconds(300).ToUnixTimeSeconds(); // 5 min expiry
            var data = $"{publicId}:{expiry}";
            var signature = CreateSignature(data);
            var token = $"{publicId}:{expiry}:{signature}";
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return $"/api/cloudinary/secure-image/{encodedToken}";
        }

        private string CreateSignature(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}