using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.MerchantServices
{
    public class GetMerchantServicesAsyncQuery : APagedAsyncQuery<MerchantService>
    {
        public IEnumerable<int>? PersonIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<MerchantServiceType>? MerchantServiceTypes { get; init; }
        public bool? UnpaidOnly { get; init; }

        public DateTime? PaymentsFrom { get; init; }
        public DateTime? PaymentsTo { get; init; }

        public bool IncludeMerchants { get; init; }
        public bool IncludePostings { get; init; }
        public bool IncludeJournals { get; init; }
    }

    internal class GetMerchantServicesAsyncQueryHandler : APagedQueryAsyncHandler<GetMerchantServicesAsyncQuery, MerchantService>
    {
        private readonly IMerchantServicesRepository repository;

        public GetMerchantServicesAsyncQueryHandler(IMerchantServicesRepository merchantServiceRepository)
        {
            repository = merchantServiceRepository;
        }
        public override Task<IPagedData<MerchantService>> Handle(GetMerchantServicesAsyncQuery query)
        {
            return repository.GetAsync(new GetMerchantServicesCriteria
            {
                PersonIds = query.PersonIds,
                MerchantIds = query.MerchantIds,
                MerchantServiceTypes = query.MerchantServiceTypes,
                UnpaidOnly = query.UnpaidOnly,
                PaymentsFrom = query.PaymentsFrom,
                PaymentsTo = query.PaymentsTo,

                IncludeMerchants = query.IncludeMerchants,
                IncludePostings = query.IncludePostings,
                IncludeJournals = query.IncludeJournals,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
