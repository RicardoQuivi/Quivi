using Microsoft.Extensions.Hosting;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;

namespace Quivi.Application.Services.Acquirers.Cash
{
    public class CashAcquirerProcessor : IAcquirerProcessor
    {
        private readonly IHostEnvironment hostEnvironment;
        private readonly IIdConverter idConverter;

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


        public CashAcquirerProcessor(IHostEnvironment hostEnvironment, IIdConverter idConverter)
        {
            this.hostEnvironment = hostEnvironment;
            this.idConverter = idConverter;
        }

        public ChargePartner ChargePartner => ChargePartner.Quivi;
        public ChargeMethod ChargeMethod => ChargeMethod.Cash;

        public Task<IStatusResult> GetStatus(Charge charge) => Task.FromResult<IStatusResult>(new CashStatusResult
        {
            GatewayId = idConverter.ToPublicId(charge.Id),
            Status = new[] { ChargeStatus.Processing, ChargeStatus.Completed }.Contains(charge.Status) ? PaymentStatus.Success : PaymentStatus.Processing,
        });

        public Task<IInitiateResult> Initiate(Charge charge) => Task.FromResult<IInitiateResult>(new CashInitiateResult());
        public Task<IProcessResult> Process(Charge charge) => hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.FromResult<IProcessResult>(new CashProcessResult
        {
            GatewayId = idConverter.ToPublicId(charge.Id),
        });

        public Task Refund(Charge charge, decimal amount) => hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.CompletedTask;

        public Task OnSetup(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
        public Task OnTearDown(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
    }
}