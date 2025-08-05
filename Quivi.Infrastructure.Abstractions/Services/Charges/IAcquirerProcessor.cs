using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Services.Charges.Results;

namespace Quivi.Infrastructure.Abstractions.Services.Charges
{
    public interface IAcquirerProcessor
    {
        ChargePartner ChargePartner { get; }
        ChargeMethod ChargeMethod { get; }

        Task<IInitiateResult> Initiate(Charge charge);
        Task<IProcessResult> Process(Charge charge);
        Task<IStatusResult> GetStatus(Charge charge);

        Task Refund(Charge charge, decimal amount);

        Task OnSetup(MerchantAcquirerConfiguration configuration);
        Task OnTearDown(MerchantAcquirerConfiguration configuration);
    }
}