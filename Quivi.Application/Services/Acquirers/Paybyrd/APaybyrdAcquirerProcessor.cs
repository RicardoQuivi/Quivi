using Paybyrd.Api;
using Paybyrd.Api.Models;
using Paybyrd.Api.Requests;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Services.Acquirers.Paybyrd
{
    public abstract class APaybyrdAcquirerProcessor : IAcquirerProcessor
    {
        public ChargePartner ChargePartner => ChargePartner.Paybyrd;
        public abstract ChargeMethod ChargeMethod { get; }

        protected readonly IPaybyrdApi paybyrdApi;
        private readonly IPaybyrdWebhooksApi paybyrdWebhooksApi;
        private readonly IAppHostsSettings appHostsSettings;
        private readonly IQueryProcessor queryProcessor;

        public APaybyrdAcquirerProcessor(IPaybyrdApi paybyrdApi,
                                            IPaybyrdWebhooksApi paybyrdWebhooksApi,
                                            IAppHostsSettings appHostsSettings,
                                            IQueryProcessor queryProcessor)
        {
            this.paybyrdApi = paybyrdApi;
            this.paybyrdWebhooksApi = paybyrdWebhooksApi;
            this.appHostsSettings = appHostsSettings;
            this.queryProcessor = queryProcessor;
        }

        public abstract Task<IInitiateResult> Initiate(Charge charge);
        public abstract Task<IProcessResult> Process(Charge charge);
        public abstract Task<IStatusResult> GetStatus(Charge charge);

        public async Task Refund(Charge charge, decimal amount)
        {
            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.RefundPayment(apiKey, new RefundPaymentRequest
            {
                TransactionId = GetAcquirerId(charge) ?? throw new Exception("No Acquirer Id!"),
                Amount = amount,
                IsoAmount = (int)(amount * 100),
            });
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

                    EventType.Order.Paid,
                    EventType.Order.Canceled,
                    EventType.Order.Expired,
                    EventType.Order.TemporaryFailed,
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

        protected static string GetApiKey(Charge charge) => charge.MerchantAcquirerConfiguration?.ApiKey ?? throw new Exception($"{nameof(MerchantAcquirerConfiguration.ApiKey)} is not set for the charge.");
        private static string? GetAcquirerId(Charge charge) => charge.AcquirerCharge?.AcquirerId;
        private string GetWebwookUrl() => appHostsSettings.Background.CombineUrl($"/api/paybyrd");
    }
}