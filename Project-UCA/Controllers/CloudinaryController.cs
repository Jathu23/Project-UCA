using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_UCA.Middleware;
using Project_UCA.Utilities.Interface;
using System;
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

            var (tempUrl, _, returnedPublicId) = await _cloudinaryService.UploadImageAsync(file, publicId);
            return Ok(new { TempUrl = tempUrl, PublicId = returnedPublicId });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            await _cloudinaryService.DeleteImageAsync(publicId);
            return Ok(null);
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetImage([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");

            var imageurl = await _cloudinaryService.GetImageById(publicId);
            return Ok(imageurl);
        }

        [HttpGet("secure-image/{token}")]
        public async Task<IActionResult> GetSecureImage(string token)
        {
            var (isValid, publicId) = _cloudinaryService.ValidateTempUrl(token);
            if (!isValid)
                throw new BadRequestException("Invalid or expired token.");

            var imageBytes = await _cloudinaryService.DownloadImageAsync(publicId);
            return File(imageBytes, "image/jpeg");
        }
    }
}