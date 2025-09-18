namespace Quivi.Pos.Api.Dtos
{
    public class BaseTransactionItem
    {
        public required string Id { get; init; }
        public required string TransactionId { get; init; }
        public decimal Quantity { get; init; }
        public required string Name { get; init; }
        public decimal OriginalPrice { get; init; }
        public decimal Price { get; init; }
        public decimal AppliedDiscountPercentage { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset LastModified { get; init; }
    }

    public class TransactionItem : BaseTransactionItem
    {
        public required IEnumerable<BaseTransactionItem> Modifiers { get; set; }
    }
}