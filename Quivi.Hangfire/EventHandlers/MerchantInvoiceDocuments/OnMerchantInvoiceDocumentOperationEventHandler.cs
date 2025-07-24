using Quivi.Application.Commands.MerchantInvoiceDocuments;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.MerchantInvoiceDocuments;
using Quivi.Infrastructure.Abstractions.Jobs;

namespace Quivi.Hangfire.EventHandlers.MerchantInvoiceDocuments
{
    public class OnMerchantInvoiceDocumentOperationEventHandler : BackgroundEventHandler<OnMerchantInvoiceDocumentOperationEvent>
    {
        private readonly ICommandProcessor commandProcessor;

        public OnMerchantInvoiceDocumentOperationEventHandler(ICommandProcessor commandProcessor,
                                                                IBackgroundJobHandler backgroundJobHandler) : base(backgroundJobHandler)
        {
            this.commandProcessor = commandProcessor;
        }

        public override Task Run(OnMerchantInvoiceDocumentOperationEvent message)
        {
            if(message.PosChargeId.HasValue == false)
                return Task.CompletedTask;

            return commandProcessor.Execute(new ProcessMerchantInvoiceDocumentAsyncCommand
            {
                MerchantInvoiceDocumentId = message.Id,
            });
        }
    }
}