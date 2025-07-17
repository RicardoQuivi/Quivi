namespace Quivi.Guests.Api.Dtos
{
    public class OrderItem : BaseOrderItem
    {
        public required IEnumerable<ModifierGroup> Modifiers { get; set; }
    }

    public class ModifierGroup
    {
        public required string Id { get; set; }
        public required IEnumerable<BaseOrderItem> SelectedOptions { get; set; }
    }

    public class BaseOrderItem
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
    }
}