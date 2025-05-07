namespace Quivi.Backoffice.Api.Requests.Storage
{
    public class UploadImageRequest : UploadFileRequest
    {
        public required string Name { get; init; }
    }
}
