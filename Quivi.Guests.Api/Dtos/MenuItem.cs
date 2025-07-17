namespace Quivi.Guests.Api.Dtos
{
    public class BaseMenuItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public decimal Price { get; init; }
        public required string PriceType { get; init; }
        public bool IsAvailable { get; init; }
    }

    public class MenuItemModifierGroup
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public int MinSelection { get; init; }
        public int MaxSelection { get; init; }

        public required IEnumerable<BaseMenuItem> Options { get; init; }
    }


    public class MenuItem : BaseMenuItem
    {
        public required IEnumerable<MenuItemModifierGroup> Modifiers { get; init; }
    }
}