namespace Quivi.Pos.Api.Dtos
{
    public class MenuItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? ImageUrl { get; init; }
        public decimal Price { get; init; }
        public required IEnumerable<ModifierGroup> ModifierGroups { get; init; }
    }
}