using Paybyrd.Api;
using Paybyrd.Api.Models;
using Paybyrd.Api.Requests;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Services.Acquirers
{
    public class PaybyrdCreditCardAcquirerProcessingStrategy : IAcquirerProcessingStrategy
    {
        private static readonly TransactionStatus[] ErrorsStatuses = [TransactionStatus.Denied, TransactionStatus.Error, TransactionStatus.Canceled];

        private readonly IIdConverter idConverter;
        private readonly IPaybyrdApi paybyrdApi;
        private readonly IPaybyrdWebhooksApi paybyrdWebhooksApi;
        private readonly IQueryProcessor queryProcessor;
        private readonly IAppHostsSettings appHostsSettings;

        public PaybyrdCreditCardAcquirerProcessingStrategy(IPaybyrdApi paybyrdApi,
                                                            IPaybyrdWebhooksApi paybyrdWebhooksApi,
                                                            IIdConverter idConverter,
                                                            IQueryProcessor queryProcessor,
                                                            IAppHostsSettings appHostsSettings)
        {
            this.paybyrdApi = paybyrdApi;
            this.paybyrdWebhooksApi = paybyrdWebhooksApi;
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.appHostsSettings = appHostsSettings;
        }

        public ChargePartner ChargePartner => ChargePartner.Paybyrd;
        public ChargeMethod ChargeMethod => ChargeMethod.CreditCard;
        public bool IsPassthrough => false;

        public async Task<Infrastructure.Abstractions.Services.Charges.PaymentStatus> GetStatus(Charge charge)
        {
            if (string.IsNullOrWhiteSpace(charge.CardCharge?.TransactionId))
                return Infrastructure.Abstractions.Services.Charges.PaymentStatus.Failed;

            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.GetPayment(apiKey, new GetPaymentRequest
            {
                TransactionId = charge.CardCharge.TransactionId,
            });

            if (response.Status == TransactionStatus.Success)
                return Infrastructure.Abstractions.Services.Charges.PaymentStatus.Success;

            if (ErrorsStatuses.Contains(response.Status))
                return Infrastructure.Abstractions.Services.Charges.PaymentStatus.Failed;

            return Infrastructure.Abstractions.Services.Charges.PaymentStatus.Processing;
        }

        public async Task<ProcessResult> Process(Charge charge)
        {
            string apiKey = GetApiKey(charge);

            var response = await paybyrdApi.CreatePayment(apiKey, new CreatePaymentRequest
            {
                Amount = charge.PosCharge!.Total,
                Currency = Currency.EUR,
                OrderRef = idConverter.ToPublicId(charge.Id),
                RedirectUrl = charge.CardCharge!.FormContext,
                Type = PaymentType.Card,
                Card = new CreatePaymentCard
                {
                    TokenId = charge.CardCharge!.AuthorizationToken,
                },
            });

            return new ProcessResult
            {
                GatewayTransactionId = response.TransactionId,
                CreditCard = new CreditCardResult
                {
                    ChallengeUrl = response.Action?.Url,
                },
            };
        }

        public Task Refund(Charge charge, decimal amount)
        {
            throw new NotImplementedException("Refund functionality is not implemented for Paybyrd credit card charges.");
        }

        public async Task OnSetup(MerchantAcquirerConfiguration configuration)
        {
            var apiKey = configuration.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return;

            var webhookSettings = await paybyrdWebhooksApi.GetSettings(apiKey);
            var url = GetWebwookUrl();
            if (webhookSettings.Data.Where(s => s.Url == url).Any())
                return;

            var response = await paybyrdWebhooksApi.Create(apiKey, new CreateWebhookRequest
            {
                Url = url,
                CredentialType = CredentialType.ApiKey,
                Events =
                [
                    EventType.Transaction.Payment.Success,
                    EventType.Transaction.Payment.Canceled,
                    EventType.Transaction.Payment.Pending,
                    EventType.Transaction.Payment.Canceled,
                    EventType.Transaction.Payment.Error,
                ],
                PaymentMethods =
                [
                    PaymentMethod.Card,
                    PaymentMethod.MbWay,
                ],
            });
        }

        public async Task OnTearDown(MerchantAcquirerConfiguration configuration)
        {
            var apiKey = configuration.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return;

            var acquirerConfigurationQuery = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                ApiKeys = [apiKey],
                IsDeleted = false,
                PageSize = 0,
            });
            if (acquirerConfigurationQuery.TotalItems > 0)
                return;

            var url = GetWebwookUrl();
            var webhookSettings = await paybyrdWebhooksApi.GetSettings(apiKey);
            foreach (var webhook in webhookSettings.Data.Where(s => s.Url == url))
                await paybyrdWebhooksApi.Delete(apiKey, new DeleteWebhookRequest
                {
                    WebhookId = webhook.Id,
                });
        }

        private static string GetApiKey(Charge charge) => charge.MerchantAcquirerConfiguration?.ApiKey ?? throw new Exception("Api Key is not set for the charge.");
        private string GetWebwookUrl() => appHostsSettings.Background.CombineUrl($"/api/paybyrd");
    }
}