using FacturaLusa.v2.Dtos;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2.Models
{
    public class SaleDocumentItem
    {
        public required string Reference { get; init; }
        public required string CorrelationId { get; init; }
        public decimal Price { get; init; }
        public decimal Quantity { get; init; }
        public decimal DiscountPercentage { get; init; }
        public decimal TaxPercentage { get; init; }
        public required string Description { get; init; }
        public bool IsGeneric { get; init; }
        public required string Name { get; init; }
        public required ItemType Type { get; init; }
    }
}