namespace Quivi.Backoffice.Api.Dtos
{
    public class TransactionItem
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public decimal Quantity { get; init; }
        public decimal FinalPrice { get; set; }
        public decimal OriginalPrice { get; set; }
    }
}