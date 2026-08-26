using Microsoft.AspNetCore.Mvc;

namespace Vendora.Api.Controllers;

public record UploadedImageDto(string Url);

/// <summary>
/// Handles product image uploads, saving the file to this machine's local disk
/// (wwwroot/uploads/products) and serving it back out via static file middleware.
/// No cloud storage yet - this app currently only ever runs on the store's own machine,
/// so "local disk" is genuinely where the data belongs for now.
/// </summary>
[ApiController]
[Route("api/uploads")]
public class UploadsController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly Dictionary<string, string> AllowedContentTypes = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    [HttpPost("product-image")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<UploadedImageDto>> UploadProductImage(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was uploaded.");
        if (file.Length > MaxFileSizeBytes)
            return BadRequest("Image must be 5 MB or smaller.");
        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var extension))
            return BadRequest("Only JPEG, PNG, WEBP, or GIF images are allowed.");

        var webRoot = string.IsNullOrEmpty(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;
        var uploadsFolder = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Ok(new UploadedImageDto($"/uploads/products/{fileName}"));
    }
}
