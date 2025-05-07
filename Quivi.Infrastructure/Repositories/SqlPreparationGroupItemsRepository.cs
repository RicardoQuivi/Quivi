using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPreparationGroupItemsRepository : ARepository<PreparationGroupItem, GetPreparationGroupItemsCriteria>, IPreparationGroupItemsRepository
    {
        public SqlPreparationGroupItemsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PreparationGroupItem> GetFilteredQueryable(GetPreparationGroupItemsCriteria criteria)
        {
            IQueryable<PreparationGroupItem> query = Set;
            return query.OrderBy(o => o.CreatedDate);
        }
    }
}