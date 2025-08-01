using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Merchants;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.MerchantAcquirerConfigurations
{
    public class GetMerchantAcquirerConfigurationsAsyncQuery : APagedAsyncQuery<MerchantAcquirerConfiguration>
    {
        public IEnumerable<int>? Ids { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public IEnumerable<int>? ChannelIds { get; init; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; init; }
        public IEnumerable<ChargePartner>? ChargePartners { get; init; }
        public IEnumerable<string>? ApiKeys { get; init; }
        public bool? IsDeleted { get; init; }
    }

    public class GetMerchantAcquirerConfigurationsAsyncQueryHandler : APagedQueryAsyncHandler<GetMerchantAcquirerConfigurationsAsyncQuery, MerchantAcquirerConfiguration>
    {
        private readonly IMerchantAcquirerConfigurationsRepository repository;

        public GetMerchantAcquirerConfigurationsAsyncQueryHandler(IMerchantAcquirerConfigurationsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<MerchantAcquirerConfiguration>> Handle(GetMerchantAcquirerConfigurationsAsyncQuery query)
        {
            return repository.GetAsync(new GetMerchantAcquirerConfigurationsCriteria
            {
                Ids = query.Ids,
                MerchantIds = query.MerchantIds,
                ChannelIds = query.ChannelIds,
                ChargeMethods = query.ChargeMethods,
                ChargePartners = query.ChargePartners,
                ApiKeys = query.ApiKeys,
                IsDeleted = query.IsDeleted,

                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}
