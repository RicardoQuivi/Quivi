namespace Quivi.Backoffice.Api.Dtos
{
    public class Sales
    {
        public required DateTimeOffset From { get; init; }
        public required DateTimeOffset To { get; init; }

        public required decimal Total { get; init; }
        public required decimal Payment { get; init; }
        public required decimal Tip { get; init; }
        public required decimal TotalRefund { get; init; }
        public required decimal PaymentRefund { get; init; }
        public required decimal TipRefund { get; init; }
    }
}