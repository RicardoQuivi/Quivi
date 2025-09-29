using Paybyrd.Api;
using Paybyrd.Api.Models;
using Paybyrd.Api.Requests;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;

namespace Quivi.Application.Services.Acquirers.Paybyrd
{
    public class PaybyrdTerminalAcquirerProcessor : APaybyrdAcquirerProcessor
    {
        protected static readonly TransactionStatus[] ErrorsStatuses = [TransactionStatus.Denied, TransactionStatus.Error, TransactionStatus.Canceled];

        private readonly IIdConverter idConverter;

        public override ChargeMethod ChargeMethod => ChargeMethod.PaymentTerminal;

        private class TerminalInitiateResult : IInitiateResult
        {
            public string? GatewayId { get; init; }
            bool IInitiateResult.CaptureStarted => true;
        }

        private class TerminalStatusResult : IStatusResult
        {
            public string? GatewayId { get; init; }
            public Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus Status { get; init; }
        }

        public PaybyrdTerminalAcquirerProcessor(IPaybyrdApi paybyrdApi,
                                                IPaybyrdWebhooksApi paybyrdWebhooksApi,
                                                IAppHostsSettings appHostsSettings,
                                                IQueryProcessor queryProcessor,
                                                IIdConverter idConverter) : base(paybyrdApi, paybyrdWebhooksApi, appHostsSettings, queryProcessor)
        {
            this.idConverter = idConverter;
        }

        public override async Task<IInitiateResult> Initiate(Charge charge)
        {
            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.CreatePayment(apiKey, new CreatePaymentRequest
            {
                Amount = charge.PosCharge!.Total,
                Currency = Currency.EUR,
                IsPreAuth = false,
                OrderRef = idConverter.ToPublicId(charge.Id),
                TerminalSerialNumber = charge.MerchantAcquirerConfiguration!.TerminalId ?? throw new Exception($"{nameof(MerchantAcquirerConfiguration.TerminalId)} is not set for the charge."),
                Type = PaymentType.POS,
            });

            charge.AcquirerCharge!.AcquirerId = response.TransactionId;
            return new TerminalInitiateResult
            {
                GatewayId = response.TransactionId,
            };
        }

        public override Task<IProcessResult> Process(Charge charge) => throw new NotSupportedException();

        public override async Task<IStatusResult> GetStatus(Charge charge)
        {
            var transactionId = charge.AcquirerCharge?.AcquirerId;

            if (string.IsNullOrWhiteSpace(transactionId))
                return new TerminalStatusResult
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
                return new TerminalStatusResult
                {
                    GatewayId = transactionId,
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Success,
                };

            if (ErrorsStatuses.Contains(response.Status))
                return new TerminalStatusResult
                {
                    GatewayId = transactionId,
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Failed,
                };

            return new TerminalStatusResult
            {
                GatewayId = transactionId,
                Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Processing,
            };
        }
    }
}