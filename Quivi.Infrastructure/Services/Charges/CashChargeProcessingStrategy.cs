using Microsoft.Extensions.Hosting;
using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges;
namespace Quivi.Infrastructure.Services.Charges
{
    public class CashChargeProcessingStrategy : IChargeProcessingStrategy
    {
        private readonly IHostEnvironment hostEnvironment;

        public CashChargeProcessingStrategy(IHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
        }

        public ChargePartner ChargePartner => ChargePartner.Quivi;
        public ChargeMethod ChargeMethod => ChargeMethod.Cash;
        public bool RefundIntoWallet => false;
        public bool IsPassthrough => false;

        public Task ProcessChargeStatus(Charge charge) => this.hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.CompletedTask;
        public Task Refund(Charge charge, decimal amount) => this.hostEnvironment.IsProduction() ? throw new Exception("Cash charge processing is not supported in production mode.") : Task.CompletedTask;
    }
}