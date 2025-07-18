namespace Quivi.Backoffice.Api.Dtos
{
    public class Review
    {
        public required string Id { get; init; }
        public int Stars { get; init; }
        public string? Comment { get; init; }
    }
}
