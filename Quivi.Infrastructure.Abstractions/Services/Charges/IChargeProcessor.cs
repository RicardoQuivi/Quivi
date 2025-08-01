using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public interface IChargeProcessor
    {
        Task<StartProcessingResult> StartProcessing(int chargeId, Action<Charge> onProcessingStart);
        Task CheckAndUpdateState(int chargeId);
        Task Refund(RefundParameters parameters);
    }
}