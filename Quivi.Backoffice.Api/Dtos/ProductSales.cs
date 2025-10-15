namespace Quivi.Backoffice.Api.Dtos
{
    public class ProductSales
    {
        public required string MenuItemId { get; init; }

        public required DateTimeOffset From { get; init; }
        public required DateTimeOffset To { get; init; }

        public decimal TotalSoldQuantity { get; init; }
        public decimal TotalBilledAmount { get; init; }
    }
}