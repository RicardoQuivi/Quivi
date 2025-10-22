namespace Quivi.Backoffice.Api.Dtos
{
    public class Settlement
    {
        public required string Id { get; init; }
        public DateOnly Date { get; init; }

        public decimal ServiceAmount { get; init; }

        public decimal GrossAmount { get; init; }
        public decimal GrossTip { get; init; }
        public decimal GrossTotal { get; init; }

        public decimal NetAmount { get; init; }
        public decimal NetTip { get; init; }
        public decimal NetTotal { get; init; }
    }
}