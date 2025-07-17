using Quivi.Domain.Entities.Merchants;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlMerchantAcquirerConfigurationsRepository : ARepository<MerchantAcquirerConfiguration, GetMerchantAcquirerConfigurationsCriteria>, IMerchantAcquirerConfigurationsRepository
    {
        public SqlMerchantAcquirerConfigurationsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<MerchantAcquirerConfiguration> GetFilteredQueryable(GetMerchantAcquirerConfigurationsCriteria criteria)
        {
            IQueryable<MerchantAcquirerConfiguration> query = Set;

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.ChannelIds != null)
            {
                var merchantIds = Context.Channels.Where(q => criteria.ChannelIds.Contains(q.Id))
                                                  .Select(q => q.MerchantId)
                                                  .Distinct();
                query = query.Where(q => merchantIds.Contains(q.MerchantId));
            }

            if (criteria.ChargePartners != null)
                query = query.Where(q => criteria.ChargePartners.Contains(q.ChargePartner));

            if (criteria.ChargeMethods != null)
                query = query.Where(q => criteria.ChargeMethods.Contains(q.ChargeMethod));

            if(criteria.IsDeleted.HasValue)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.Value);

            return query.OrderByDescending(q => q.CreatedDate);
        }
    }
}