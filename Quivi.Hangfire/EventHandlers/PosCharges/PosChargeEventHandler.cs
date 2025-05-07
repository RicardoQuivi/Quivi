using Quivi.Application.Extensions;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Pos;
using static Quivi.Application.Extensions.PosChargeExtensions;

namespace Quivi.Hangfire.EventHandlers.PosCharges
{
    public class PosChargeEventHandler : BackgroundEventHandler<OnPosChargeOperationEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IPosSyncService posSyncService;

        public PosChargeEventHandler(IQueryProcessor queryProcessor,
                                        ICommandProcessor commandProcessor,
                                        IPosSyncService posSyncService,
                                        IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.posSyncService = posSyncService;
        }

        public override async Task Run(OnPosChargeOperationEvent message)
        {
            var posChargesQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [message.Id],
                PageIndex = 0,
                PageSize = 1,
            });
            var posCharge = posChargesQuery.Single();

            //if (posCharge.SurchargeFeeAmount > 0)
            //{
            //    var processDeGrazieInvoiceJob = backgroundJobHandler.Enqueue(() => _invoiceService.ProcessSurchargeInvoice(charge.ChargeId));
            //    if (string.IsNullOrWhiteSpace(posCharge.Email) == false)
            //        backgroundJobHandler.ContinueJobWith(processDeGrazieInvoiceJob, () => SendPaymentConfirmation(charge.ChargeId));
            //}
            //else if (string.IsNullOrWhiteSpace(posCharge.Email) == false)
            //    backgroundJobHandler.Enqueue(() => SendPaymentConfirmation(charge.ChargeId));

            switch (posCharge.GetPosChargeType())
            {
                case PosChargeType.FreePayment: return;
                case PosChargeType.PayAtTheTable: ValidateAndSyncCharge(posCharge); break;
            }
        }

        private void ValidateAndSyncCharge(PosCharge posCharge)
        {
            if (posCharge.SessionId.HasValue == false)
                throw new Exception($"PosCharge with Id {posCharge.Id} has no session Id.");

            backgroundJobHandler.Enqueue(() => posSyncService.ProcessCharge(posCharge.Id));
        }
    }
}