using System.Net;
using Infrastructure.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedCvExtensions = { ".pdf", ".doc", ".docx" };
    private static readonly string[] AllowedPhotoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("cv")]
    public async Task<Response<string>> UploadCvAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return new Response<string>(HttpStatusCode.BadRequest, "No file uploaded");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedCvExtensions.Contains(ext))
            return new Response<string>(HttpStatusCode.BadRequest, "Allowed formats: PDF, DOC, DOCX");

        if (file.Length > MaxFileSize)
            return new Response<string>(HttpStatusCode.BadRequest, "File too large (max 5 MB)");

        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(root, "uploads", "cv");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(stream);

        var url = $"/uploads/cv/{fileName}";
        return new Response<string>(HttpStatusCode.OK, "CV uploaded", url);
    }

    [HttpPost("photo")]
    public async Task<Response<string>> UploadPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return new Response<string>(HttpStatusCode.BadRequest, "No file uploaded");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(ext))
            return new Response<string>(HttpStatusCode.BadRequest, "Allowed formats: JPG, PNG, GIF, WEBP");

        if (file.Length > MaxFileSize)
            return new Response<string>(HttpStatusCode.BadRequest, "File too large (max 5 MB)");

        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(root, "uploads", "photos");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(stream);

        var url = $"/uploads/photos/{fileName}";
        return new Response<string>(HttpStatusCode.OK, "Photo uploaded", url);
    }
}
