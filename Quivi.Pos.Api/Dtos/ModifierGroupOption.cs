namespace Quivi.Pos.Api.Dtos
{
    public class ModifierGroupOption
    {
        public required string Id { get; init; }
        public required string MenuItemId { get; init; }
        public decimal Price { get; init; }
    }
}