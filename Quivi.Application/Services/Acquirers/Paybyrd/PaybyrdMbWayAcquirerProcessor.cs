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
    public class PaybyrdMbWayAcquirerProcessor : APaybyrdAcquirerProcessor
    {
        protected static readonly OrderStatus[] ErrorsStatuses = [OrderStatus.Expired, OrderStatus.Canceled];

        public override ChargeMethod ChargeMethod => ChargeMethod.MbWay;

        private readonly IIdConverter idConverter;

        private class MbWayInitiateResult : IInitiateResult
        {
            string? IInitiateResult.GatewayId => null;
            bool IInitiateResult.CaptureStarted => true;

            public required string OrderId { get; init; }
            public required string CheckoutKey { get; init; }
            public string? Culture { get; init; }
        }

        private class MbWayProcessResult : IProcessResult
        {
            string? IProcessResult.GatewayId => this.GatewayId;
            string? IProcessResult.ChallengeUrl => null;

            public required string GatewayId { get; init; }
            public required string CheckoutUrl { get; init; }
        }

        private class MbWayStatusResult : IStatusResult
        {
            public Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus Status { get; init; }
            public string? GatewayId { get; init; }
        }

        public PaybyrdMbWayAcquirerProcessor(IPaybyrdApi paybyrdApi,
                                                IPaybyrdWebhooksApi paybyrdWebhooksApi,
                                                IIdConverter idConverter,
                                                IAppHostsSettings appHostsSettings,
                                                IQueryProcessor queryProcessor) : base(paybyrdApi, paybyrdWebhooksApi, appHostsSettings, queryProcessor)
        {
            this.idConverter = idConverter;
        }

        public override async Task<IInitiateResult> Initiate(Charge charge)
        {
            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.CreateOrder(apiKey, new CreateOrderRequest
            {
                IsoAmount = (int)(charge.PosCharge!.Total * 100),
                Currency = Currency.EUR,
                OrderRef = idConverter.ToPublicId(charge.Id),
                OrderOptions = new OrderOptions
                {
                },
                Shopper = new Shopper
                {
                    Email = string.IsNullOrEmpty(charge.PosCharge!.Email) ? null : charge.PosCharge!.Email,
                },
                AllowedPaymentMethods = ["MBWAY"],
            });

            return new MbWayInitiateResult
            {
                OrderId = response.OrderId,
                CheckoutKey = response.CheckoutKey,
                Culture = response.OrderOptions.Culture,
            };
        }

        public override Task<IProcessResult> Process(Charge charge) => throw new NotSupportedException();

        public override async Task<IStatusResult> GetStatus(Charge charge)
        {
            var context = JsonSerializer.Deserialize<MbWayInitiateResult>(charge.AcquirerCharge!.AdditionalJsonContext!)!;

            if (string.IsNullOrWhiteSpace(context.OrderId))
                return new MbWayStatusResult
                {
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Failed,
                    GatewayId = null,
                };

            string apiKey = GetApiKey(charge);
            var response = await paybyrdApi.GetOrder(apiKey, new GetOrderRequest
            {
                OrderId = context.OrderId,
            });

            if (response.Status == OrderStatus.Paid)
                return new MbWayStatusResult
                {
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Success,
                    GatewayId = response?.LastTransactionId,
                };

            if (ErrorsStatuses.Contains(response.Status))
                return new MbWayStatusResult
                {
                    Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Failed,
                    GatewayId = response?.LastTransactionId,
                };

            return new MbWayStatusResult
            {
                Status = Infrastructure.Abstractions.Services.Charges.Results.PaymentStatus.Processing,
                GatewayId = response?.LastTransactionId,
            };
        }
    }
}