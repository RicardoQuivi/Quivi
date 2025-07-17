namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public class PreparationRequestParameters
    {
        public string? OrderPlaceholder { get; init; }
        public required string SessionPlaceholder { get; init; }
        public required string ChannelPlaceholder { get; init; }
        public IEnumerable<KeyValuePair<string, string>>? AdditionalInfo { get; init; }
        public required string Title { get; init; }
        public DateTime Timestamp { get; init; }
        public required IList<PreparationRequestItem> Items { get; init; }
    }

    public class BasePreparationRequestItem
    {
        public required string Name { get; init; }
        public int Quantity { get; init; }
    }

    public class PreparationRequestItem : BasePreparationRequestItem
    {
        public bool? Add { get; set; }
        public IEnumerable<BasePreparationRequestItem>? Modifiers { get; init; }
    }
}