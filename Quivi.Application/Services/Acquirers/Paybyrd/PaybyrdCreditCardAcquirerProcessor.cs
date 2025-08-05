using Paybyrd.Api;
using Paybyrd.Api.Models;
using Paybyrd.Api.Requests;
using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;
using System.Text.Json;

namespace Quivi.Application.Services.Acquirers.Paybyrd
{
    public class PaybyrdCreditCardAcquirerProcessor : APaybyrdAcquirerProcessor
    {
        protected static readonly TransactionStatus[] ErrorsStatuses = [TransactionStatus.Denied, TransactionStatus.Error, TransactionStatus.Canceled];

        public override ChargeMethod ChargeMethod => ChargeMethod.CreditCard;

        private readonly IIdConverter idConverter;

        private class CreditCardInitiateResult : IInitiateResult
        {
            string? IInitiateResult.GatewayId => null;
            bool IInitiateResult.CaptureStarted => false;
        }

        private class CreditCardProcessResult : IProcessResult
        {
            string? IProcessResult.GatewayId => this.GatewayId;

            public required string GatewayId { get; init; }
            public string? ChallengeUrl { get; init; } = string.Empty;
        }

        private class CreditCardStatusResult : IStatusResult
        {
            public string? GatewayId { get; init; }
            public Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus Status { get; init; }
        }

        private class CreditCardContext
        {
            public required string TokenId { get; init; }
            public required string RedirectUrl { get; init; }
        }

        public PaybyrdCreditCardAcquirerProcessor(IPaybyrdApi paybyrdApi,
                                                            IPaybyrdWebhooksApi paybyrdWebhooksApi,
                                                            IIdConverter idConverter,
                                                            IQueryProcessor queryProcessor,
                                                            IAppHostsSettings appHostsSettings) : base(paybyrdApi, paybyrdWebhooksApi, appHostsSettings, queryProcessor)
        {
            this.idConverter = idConverter;
        }

        public override Task<IInitiateResult> Initiate(Charge charge) => Task.FromResult<IInitiateResult>(new CreditCardInitiateResult());

        public override async Task<IProcessResult> Process(Charge charge)
        {
            string apiKey = GetApiKey(charge);

            var context = JsonSerializer.Deserialize<CreditCardContext>(charge.AcquirerCharge!.AdditionalJsonContext!)!;
            var response = await paybyrdApi.CreatePayment(apiKey, new CreatePaymentRequest
            {
                Amount = charge.PosCharge!.Total,
                Currency = Currency.EUR,
                OrderRef = idConverter.ToPublicId(charge.Id),
                RedirectUrl = context.RedirectUrl,
                Type = PaymentType.Card,
                Card = new CreatePaymentCard
                {
                    TokenId = context.TokenId,
                },
            });

            charge.AcquirerCharge.AcquirerId = response.TransactionId;
            return new CreditCardProcessResult
            {
                GatewayId = response.TransactionId,
                ChallengeUrl = response.Action?.Url,
            };
        }

        public override async Task<IStatusResult> GetStatus(Charge charge)
        {
            var transactionId = charge.AcquirerCharge?.AcquirerId;

            if (string.IsNullOrWhiteSpace(transactionId))
                return new CreditCardStatusResult
                {
                    GatewayId = null,
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Failed,
                };

            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.GetPayment(apiKey, new GetPaymentRequest
            {
                TransactionId = transactionId,
            });

            if (response.Status == TransactionStatus.Success)
                return new CreditCardStatusResult
                {
                    GatewayId = transactionId,
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Success,
                };

            if (ErrorsStatuses.Contains(response.Status))
                return new CreditCardStatusResult
                {
                    GatewayId = transactionId,
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Failed,
                };

            return new CreditCardStatusResult
            {
                GatewayId = transactionId,
                Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Processing,
            };
        }
    }
}