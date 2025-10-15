namespace Quivi.Backoffice.Api.Dtos
{
    public class CategorySales
    {
        public required string MenuCategoryId { get; init; }

        public required DateTimeOffset From { get; init; }
        public required DateTimeOffset To { get; init; }

        public decimal TotalItemsSoldQuantity { get; init; }
        public decimal TotalBilledAmount { get; init; }
    }
}