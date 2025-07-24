using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Repositories.EntityFramework.Identity;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Events;
using Quivi.Infrastructure.Abstractions.Events.Data.PosCharges;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using Quivi.Infrastructure.Extensions;
using System;
using System.Net;

namespace Quivi.Hangfire.EventHandlers.PosCharges
{
    public class OnPosChargeCapturedEventHandler : BackgroundEventHandler<OnPosChargeCapturedEvent>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IEmailEngine emailEngine;
        private readonly IEmailService emailService;
        private readonly IIdConverter idConverter;

        public OnPosChargeCapturedEventHandler(IBackgroundJobHandler backgroundJobHandler,
                                                IQueryProcessor queryProcessor,
                                                IEmailEngine emailEngine,
                                                IEmailService emailService,
                                                IIdConverter idConverter) : base(backgroundJobHandler)
        {
            this.queryProcessor = queryProcessor;
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
            if (string.IsNullOrWhiteSpace(posCharge.Email))
                return;

            await emailService.SendAsync(new MailMessage
            {
                ToAddress = posCharge.Email,
                Subject = $"Confirmação de compra no estabelecimento {posCharge.Merchant!.Name}",
                Body = emailEngine.PurchaseConfirmation(new PurchaseConfirmationParameters
                {
                    Date = posCharge.CaptureDate!.Value.ToTimeZone(posCharge.Merchant!.TimeZone),
                    Amount = posCharge.Payment + posCharge.Tip + posCharge.SurchargeFeeAmount,
                    TransactionId = idConverter.ToPublicId(posCharge.Id),
                    MerchantName = posCharge.Merchant!.Name,
                }),
            });
        }
    }
}