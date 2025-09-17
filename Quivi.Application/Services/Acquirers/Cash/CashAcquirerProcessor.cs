using Microsoft.Extensions.Hosting;
using Quivi.Application.Queries.Merchants;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;

namespace Quivi.Application.Services.Acquirers.Cash
{
    public class CashAcquirerProcessor : IAcquirerProcessor
    {
        private readonly IHostEnvironment hostEnvironment;
        private readonly IIdConverter idConverter;
        private readonly IQueryProcessor queryProcessor;

        private class CashInitiateResult : IInitiateResult
        {
            string? IInitiateResult.GatewayId => null;
            bool IInitiateResult.CaptureStarted => false;
        }

        private class CashProcessResult : IProcessResult
        {
            public string? GatewayId { get; init; }
            public string? ChallengeUrl => null;
        }

        private class CashStatusResult : IStatusResult
        {
            string? IStatusResult.GatewayId => GatewayId;
            public required string GatewayId { get; init; }
            public PaymentStatus Status { get; init; }
        }


        public CashAcquirerProcessor(IHostEnvironment hostEnvironment, IIdConverter idConverter, IQueryProcessor queryProcessor)
        {
            this.hostEnvironment = hostEnvironment;
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
        }

        public ChargePartner ChargePartner => ChargePartner.Quivi;
        public ChargeMethod ChargeMethod => ChargeMethod.Cash;

        public Task<IStatusResult> GetStatus(Charge charge) => Task.FromResult<IStatusResult>(new CashStatusResult
        {
            GatewayId = idConverter.ToPublicId(charge.Id),
            Status = new[] { ChargeStatus.Processing, ChargeStatus.Completed }.Contains(charge.Status) ? PaymentStatus.Success : PaymentStatus.Processing,
        });

        public Task<IInitiateResult> Initiate(Charge charge) => Task.FromResult<IInitiateResult>(new CashInitiateResult());
        public async Task<IProcessResult> Process(Charge charge)
        {
            if (hostEnvironment.IsProduction() == false)
                return new CashProcessResult
                {
                    GatewayId = idConverter.ToPublicId(charge.Id),
                };

            if (charge.PosCharge == null)
                throw new Exception("Cash charge processing is not supported in production mode.");

            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = [charge.PosCharge.MerchantId],
                PageSize = 1,
            });
            var merchant = merchantQuery.FirstOrDefault();
            if (merchant?.IsDemo == true)
                return new CashProcessResult
                {
                    GatewayId = idConverter.ToPublicId(charge.Id),
                };

            throw new Exception("Cash charge processing is not supported in production mode.");
        }

        public async Task Refund(Charge charge, decimal amount)
        {
            if (hostEnvironment.IsProduction() == false)
                return;

            if (charge.PosCharge == null)
                throw new Exception("Cash charge processing is not supported in production mode.");

            var merchantQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = [charge.PosCharge.MerchantId],
                PageSize = 1,
            });
            var merchant = merchantQuery.FirstOrDefault();
            if (merchant?.IsDemo == true)
                return;

            throw new Exception("Cash charge processing is not supported in production mode.");
        }

        public Task OnSetup(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
        public Task OnTearDown(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
    }
}