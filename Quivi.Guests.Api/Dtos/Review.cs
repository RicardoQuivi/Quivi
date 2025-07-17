namespace Quivi.Guests.Api.Dtos
{
    public class Review
    {
        public required string Id { get; init; }
        public string? Comment { get; init; }
        public int Stars { get; init; }
    }
}