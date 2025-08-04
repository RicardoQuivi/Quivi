using Microsoft.Extensions.Hosting;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Services.Charges;

namespace Quivi.Application.Services.Acquirers
{
    public class CashAcquirerProcessor : IAcquirerProcessor
    {
        private readonly IHostEnvironment hostEnvironment;
        private readonly IIdConverter idConverter;

        public CashAcquirerProcessor(IHostEnvironment hostEnvironment, IIdConverter idConverter)
        {
            this.hostEnvironment = hostEnvironment;
            this.idConverter = idConverter;
        }

        public ChargePartner ChargePartner => ChargePartner.Quivi;
        public ChargeMethod ChargeMethod => ChargeMethod.Cash;
        public bool IsPassthrough => false;

        public Task<PaymentStatus> GetStatus(Charge charge) => Task.FromResult(new[] { ChargeStatus.Processing, ChargeStatus.Completed }.Contains(charge.Status) ? PaymentStatus.Success : PaymentStatus.Processing);

        public Task<ProcessResult> Process(Charge charge) => this.hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.FromResult(new ProcessResult
        {
            GatewayTransactionId = idConverter.ToPublicId(charge.Id),
        });

        public Task Refund(Charge charge, decimal amount) => this.hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.CompletedTask;

        public Task OnSetup(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
        public Task OnTearDown(MerchantAcquirerConfiguration configuration) => Task.CompletedTask;
    }
}