using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Services.Charges.Parameters;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public interface IAcquirerChargeProcessor
    {
        Task<PosCharge?> Create(CreateParameters parameters);
        Task<StartProcessingResult> Process(int chargeId, Func<object?> onStartProcessingContext);
        Task CheckAndUpdateState(int chargeId);
        Task Refund(RefundParameters parameters);
    }
}