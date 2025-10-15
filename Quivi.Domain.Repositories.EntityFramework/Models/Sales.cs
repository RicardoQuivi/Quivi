namespace Quivi.Domain.Repositories.EntityFramework.Models
{
    public class Sales
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }

        public decimal Total { get; init; }
        public decimal Payment { get; init; }
        public decimal Tip { get; init; }
        public decimal TotalRefund { get; init; }
        public decimal PaymentRefund { get; init; }
        public decimal TipRefund { get; init; }
    }
}