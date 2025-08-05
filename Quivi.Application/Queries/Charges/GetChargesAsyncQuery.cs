using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Charges
{
    public class GetChargesAsyncQuery : APagedAsyncQuery<Charge>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<ChargeStatus>? Statuses { get; init; }
        public IEnumerable<ChargeMethod>? Methods { get; init; }

        public bool IncludeDeposit { get; init; }
        public bool IncludeDepositDepositCapture { get; init; }
        public bool IncludeDepositCaptureJournal { get; init; }
        public bool IncludeDepositDepositSurcharge { get; init; }
        public bool IncludeDepositSurchargeJournal { get; init; }
        public bool IncludePosCharge { get; init; }
        public bool IncludeDepositConsumer { get; init; }
        public bool IncludeChainedCharge { get; init; }
        public bool IncludeDepositDepositCapturePerson { get; init; }
        public bool IncludeAcquirerCharge { get; init; }
        public bool IncludeMerchantAcquirerConfiguration { get; init; }
    }

    public class GetChargesAsyncQueryHandler : APagedQueryAsyncHandler<GetChargesAsyncQuery, Charge>
    {
        private readonly IChargesRepository repository;

        public GetChargesAsyncQueryHandler(IChargesRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Charge>> Handle(GetChargesAsyncQuery query)
        {
            return repository.GetAsync(new Infrastructure.Abstractions.Repositories.Criterias.GetChargesCriteria
            {
                Ids = query.Ids,
                Statuses = query.Statuses,
                Methods = query.Methods,

                IncludeDeposit = query.IncludeDeposit,
                IncludeDepositDepositCapture = query.IncludeDepositDepositCapture,
                IncludeDepositCaptureJournal = query.IncludeDepositCaptureJournal,
                IncludeDepositDepositSurcharge = query.IncludeDepositDepositSurcharge,
                IncludeDepositSurchargeJournal = query.IncludeDepositSurchargeJournal,
                IncludePosCharge = query.IncludePosCharge,
                IncludeDepositConsumer = query.IncludeDepositConsumer,
                IncludeChainedCharge = query.IncludeChainedCharge,
                IncludeDepositDepositCapturePerson = query.IncludeDepositDepositCapturePerson,
                IncludeAcquirerCharge = query.IncludeAcquirerCharge,
                IncludeMerchantAcquirerConfiguration = query.IncludeMerchantAcquirerConfiguration,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
