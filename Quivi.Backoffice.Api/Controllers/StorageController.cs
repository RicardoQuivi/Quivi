using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Quivi.Backoffice.Api.Dtos;
using Quivi.Backoffice.Api.Requests.Storage;
using Quivi.Backoffice.Api.Responses.Storage;
using Quivi.Infrastructure.Abstractions.Storage;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _fileStorageService;

        public StorageController(IStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{**name}")]
        public async Task<IActionResult> Get(string name)
        {
            var fileStream = await _fileStorageService.GetFileAsync(name);
            if (fileStream == null)
                return NotFound();

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(name, out var contentType))
                contentType = "application/octet-stream";

            return File(fileStream, contentType, name);
        }

        [HttpPost]
        public async Task<UploadFileResponse> Upload(UploadFileRequest request)
        {
            var bytes = Convert.FromBase64String(request.Base64Data);
            var stream = new MemoryStream(bytes);

            var name = $"{Guid.NewGuid()}_original{GetExtension(request.Extension)}";
            var fileUrl = await _fileStorageService.SaveFile(stream, name);

            return new UploadFileResponse
            {
                Data = fileUrl,
            };
        }

        [HttpPost("image")]
        public async Task<UploadFileResponse> UploadImage(UploadImageRequest request)
        {
            var bytes = Convert.FromBase64String(request.Base64Data);
            var stream = new MemoryStream(bytes);

            var fileUrl = await _fileStorageService.SaveImage(stream, request.Name);

            return new UploadFileResponse
            {
                Data = fileUrl,
            };
        }

        private string GetExtension(FileExtension extension)
        {
            switch (extension)
            {
                case FileExtension.PDF: return ".pdf";
                case FileExtension.PNG: return ".png";
                case FileExtension.JPG: return ".jpg";
                case FileExtension.JPEG: return ".jpeg";
            }
            return "." + extension.ToString();
        }
    }
}
