namespace Quivi.Backoffice.Api.Dtos
{
    public class MerchantDocument
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string DownloadUrl { get; init; }
    }
}