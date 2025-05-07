namespace Quivi.Pos.Api.Dtos
{
    public class MenuCategory
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? ImageUrl { get; init; }
    }
}
