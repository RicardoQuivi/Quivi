using Quivi.Application.Queries.PosCharges;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;

namespace Quivi.Hangfire.EventHandlers.PosCharges
{
    public class OnPosChargeRefundedEventHandler : BackgroundEventHandler<OnPosChargeRefundedEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IPosSyncService posSyncService;

        public OnPosChargeRefundedEventHandler(IBackgroundJobHandler backgroundJobHandler,
                                                IPosSyncService posSyncService,
                                                IQueryProcessor queryProcessor) : base(backgroundJobHandler)
        {
            this.posSyncService = posSyncService;
            this.queryProcessor = queryProcessor;
        }

        public override async Task Run(OnPosChargeRefundedEvent message)
        {
            var posChargeQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [message.Id],
                MerchantIds = [message.MerchantId],
                IsCaptured = true,
                HasRefunds = true,
                PageSize = 1,
            });

            var posCharge = posChargeQuery.SingleOrDefault();
            if (posCharge == null)
                return;

            if (posCharge.PaymentRefund.HasValue == false)
                return;

            backgroundJobHandler.Enqueue(() => posSyncService.RefundCharge(posCharge.ChargeId, posCharge.PaymentRefund.Value));
        }
    }
}