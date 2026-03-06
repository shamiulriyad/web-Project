using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.API.Security;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/upload")]
    [Route("upload")]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public UploadController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("product-image")]
        [SellerOnly]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> ProductImage([FromForm] IFormFile file, [FromQuery] string provider = "local")
        {
            if (file == null || file.Length == 0) return BadRequest("File is required");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                return BadRequest("Only image files are allowed");

            if (provider.Equals("cloudinary", StringComparison.OrdinalIgnoreCase))
            {
                var cloudResult = await TryUploadToCloudinary(file);
                if (cloudResult != null) return Ok(new { provider = "cloudinary", url = cloudResult });
                return BadRequest("Cloudinary is not configured. Use provider=local.");
            }

            if (provider.Equals("s3", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(501, "S3 direct upload is not configured yet. Use Cloudinary or local fallback.");
            }

            var localUrl = await SaveLocally(file);
            var absoluteUrl = $"{Request.Scheme}://{Request.Host}{localUrl}";
            return Ok(new { provider = "local", url = absoluteUrl, relativeUrl = localUrl });
        }

        private async Task<string?> TryUploadToCloudinary(IFormFile file)
        {
            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
                return null;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signatureBase = $"timestamp={timestamp}{apiSecret}";
            var signature = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(signatureBase))).ToLowerInvariant();

            using var form = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.FileName);
            form.Add(new StringContent(apiKey), "api_key");
            form.Add(new StringContent(timestamp), "timestamp");
            form.Add(new StringContent(signature), "signature");

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"https://api.cloudinary.com/v1_1/{cloudName}/image/upload", form);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("secure_url", out var secureUrl)) return null;
            return secureUrl.GetString();
        }

        private async Task<string> SaveLocally(IFormFile file)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            Directory.CreateDirectory(uploadsPath);

            var filename = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            var fullPath = Path.Combine(uploadsPath, filename);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/products/{filename}";
        }
    }
}