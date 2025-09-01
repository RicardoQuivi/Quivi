using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using System.Linq.Expressions;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public class PosChargesResume
    {
        public decimal PaymentAmount { get; set; }
        public decimal? PaymentDiscount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal SurchageAmount { get; set; }
        public decimal TipAmount { get; set; }
    }


    public interface IPosChargesRepository : IRepository<PosCharge, GetPosChargesCriteria>
    {
        Task<IReadOnlyDictionary<T, PosChargesResume>> GetResumeAsync<T>(GetPosChargesResumeCriteria criteria, Expression<Func<PosCharge, T>> grouping, T defaultKey);
    }
}