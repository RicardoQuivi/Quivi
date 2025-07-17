using Quivi.Domain.Entities.Charges;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public interface IChargeProcessingStrategy
    {
        ChargePartner ChargePartner { get; }
        ChargeMethod ChargeMethod { get; }

        bool RefundIntoWallet { get; }
        bool IsPassthrough { get; }

        Task Refund(Charge charge, decimal amount);
        Task ProcessChargeStatus(Charge charge);
    }
}