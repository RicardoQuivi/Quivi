using Quivi.Backoffice.Api.Dtos;

namespace Quivi.Backoffice.Api.Requests.Storage
{
    public class UploadFileRequest
    {
        public FileExtension Extension { get; set; }
        public required string Base64Data { get; set; }
    }
}
