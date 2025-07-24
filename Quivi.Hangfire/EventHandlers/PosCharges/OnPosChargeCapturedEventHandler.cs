using Quivi.Application.Commands.MerchantInvoiceDocuments;
using Quivi.Application.Queries.PosCharges;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Hangfire.EventHandlers.PosCharges
{
    public class OnPosChargeCapturedEventHandler : BackgroundEventHandler<OnPosChargeCapturedEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IEmailEngine emailEngine;
        private readonly IEmailService emailService;
        private readonly IIdConverter idConverter;

        public OnPosChargeCapturedEventHandler(IBackgroundJobHandler backgroundJobHandler,
                                                IQueryProcessor queryProcessor,
                                                ICommandProcessor commandProcessor,
                                                IEmailEngine emailEngine,
                                                IEmailService emailService,
                                                IIdConverter idConverter) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.emailEngine = emailEngine;
            this.emailService = emailService;
            this.idConverter = idConverter;
        }

        public override async Task Run(OnPosChargeCapturedEvent message)
        {
            var posChargeQuery = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [message.Id],
                IsCaptured = true,

                IncludeMerchant = true,

                PageSize = 1,
                PageIndex = 0,
            });
            var posCharge = posChargeQuery.Single();
            if (string.IsNullOrWhiteSpace(posCharge.Email) == false)
                this.backgroundJobHandler.Enqueue(() => SendPaymentConfirmation(posCharge.Email, posCharge.Merchant!.Name, posCharge.CaptureDate!.Value.ToTimeZone(posCharge.Merchant!.TimeZone), posCharge.Payment + posCharge.Tip + posCharge.SurchargeFeeAmount, idConverter.ToPublicId(posCharge.Id)));
            if (posCharge.SurchargeFeeAmount > 0)
                this.backgroundJobHandler.Enqueue(() => ProcessSurchargeInvoice(posCharge.Id));
        }

        public Task SendPaymentConfirmation(string email, string merchantName, DateTime date, decimal amount, string id) => emailService.SendAsync(new MailMessage
        {
            ToAddress = email,
            Subject = $"{merchantName} - Confirmação de compra",
            Body = emailEngine.PurchaseConfirmation(new PurchaseConfirmationParameters
            {
                Date = date,
                Amount = amount,
                TransactionId = id,
                MerchantName = merchantName,
            }),
        });

        public Task ProcessSurchargeInvoice(int posChargeId) => commandProcessor.Execute(new CreateSurchageSimplifiedInvoiceAsyncCommand
        {
            PosChargeId = posChargeId,
        });
    }
}