using Microsoft.AspNetCore.Mvc;
using Project_UCA.Middleware;
using Project_UCA.Utilities.Interface;
using System.Threading.Tasks;

namespace Project_UCA.Controllers
{
    [Route("api/cloudinary")]
    [ApiController]
    public class CloudinaryTestController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public CloudinaryTestController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string publicId)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file provided.");
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            var (tempUrl, originalUrl, returnedPublicId) = await _cloudinaryService.UploadImageAsync(file, publicId);
            return Ok(new
            {
                TempUrl = tempUrl,
                OriginalUrl = originalUrl, // Note: This is returned for server use; remove if not needed by client
                PublicId = returnedPublicId
            });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");
            await _cloudinaryService.DeleteImageAsync(publicId);
            return Ok(new { Message = "Image deleted successfully." });
        }

        [HttpGet("url")]
        public async Task<IActionResult> GetImageUrl([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");
            var tempUrl = await _cloudinaryService.GetImageAsync(publicId);
            return Ok(new { TempUrl = tempUrl });
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetImageAsBytes([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");
            var imageBytes = await _cloudinaryService.DownloadImageAsync(publicId);
            return File(imageBytes, "image/jpeg"); // Adjust content type as needed
        }

        [HttpGet("secure-image/{encodedToken}")]
        public async Task<IActionResult> GetSecureImage(string encodedToken)
        {
            if (string.IsNullOrWhiteSpace(encodedToken))
                throw new BadRequestException("Invalid token.");

            var (isValid, publicId) = _cloudinaryService.ValidateTempUrl(encodedToken);
            if (!isValid)
                return BadRequest(new { Message = "Invalid or expired URL." });

            var imageBytes = await _cloudinaryService.DownloadImageAsync(publicId);
            return File(imageBytes, "image/jpeg"); // Adjust content type as needed
        }

        [HttpGet("validate")]
        public IActionResult ValidateTempUrl([FromQuery] string tempUrl)
        {
            if (string.IsNullOrWhiteSpace(tempUrl))
                throw new BadRequestException("Temp URL is required.");

            var uri = new Uri(tempUrl);
            var encodedToken = uri.Segments[^1]; // Get the last segment (encoded token)
            var (isValid, publicId) = _cloudinaryService.ValidateTempUrl(encodedToken);
            return Ok(new { IsValid = isValid, PublicId = publicId });
        }
    }
}