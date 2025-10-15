namespace Quivi.Domain.Repositories.EntityFramework.Models
{
    public abstract class ASales
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }

        public decimal TotalQuantity { get; init; }
        public decimal TotalBilledAmount { get; init; }
    }
}