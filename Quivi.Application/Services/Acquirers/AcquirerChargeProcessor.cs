using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Parameters;

namespace Quivi.Application.Services.Acquirers
{
    public class AcquirerChargeProcessor : IAcquirerChargeProcessor
    {
        private readonly AcquirerCreateChargeProcessor createChargeProcessor;
        private readonly AcquirerChargeStatusProcessor acquirerChargeStatusProcessor;
        private readonly AcquirerRefundChargeProcessor refundChargeProcessor;

        public AcquirerChargeProcessor(AcquirerCreateChargeProcessor createChargeProcessor,
                                        AcquirerChargeStatusProcessor acquirerChargeStatusProcessor,
                                        AcquirerRefundChargeProcessor refundChargeProcessor)
        {
            this.createChargeProcessor = createChargeProcessor;
            this.acquirerChargeStatusProcessor = acquirerChargeStatusProcessor;
            this.refundChargeProcessor = refundChargeProcessor;
        }

        public async Task<PosCharge?> Create(CreateParameters parameters)
        {
            var posCharge = await createChargeProcessor.CreateAsync(parameters);
            if (posCharge == null)
                return null;

            if (posCharge.Charge!.Status == ChargeStatus.Processing)
                acquirerChargeStatusProcessor.SchedulePolling(posCharge.Id);

            return posCharge;
        }

        public Task<StartProcessingResult> Process(int chargeId, Func<object?> onStartProcessingContext) => acquirerChargeStatusProcessor.ProcessAsync(chargeId, onStartProcessingContext);

        public Task CheckAndUpdateState(int chargeId) => acquirerChargeStatusProcessor.CheckAndUpdateStateAsync(chargeId);

        public Task Refund(RefundParameters parameters) => refundChargeProcessor.RefundAsync(parameters);
    }
}