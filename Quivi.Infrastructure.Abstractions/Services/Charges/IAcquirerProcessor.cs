using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public interface IAcquirerProcessor
    {
        ChargePartner ChargePartner { get; }
        ChargeMethod ChargeMethod { get; }

        bool IsPassthrough { get; }

        Task Refund(Charge charge, decimal amount);
        Task<ProcessResult> Process(Charge charge);
        Task<PaymentStatus> GetStatus(Charge charge);

        Task OnSetup(MerchantAcquirerConfiguration configuration);
        Task OnTearDown(MerchantAcquirerConfiguration configuration);
    }
}