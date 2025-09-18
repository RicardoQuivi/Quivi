namespace Quivi.Backoffice.Api.Dtos
{
    public class BaseTransactionItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public decimal Quantity { get; init; }
        public decimal OriginalPrice { get; init; }
        public decimal FinalPrice { get; init; }
    }

    public class TransactionItem : BaseTransactionItem
    {
        public required IEnumerable<BaseTransactionItem> Modifiers { get; set; }
    }
}