using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPrinterNotificationsContactRepository : ARepository<PrinterNotificationsContact, GetPrinterNotificationsContactsCriteria>, IPrinterNotificationsContactsRepository
    {
        public SqlPrinterNotificationsContactRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PrinterNotificationsContact> GetFilteredQueryable(GetPrinterNotificationsContactsCriteria criteria)
        {
            IQueryable<PrinterNotificationsContact> query = Set;

            if (criteria.IncludeNotificationsContact)
                query = query.Include(q => q.BaseNotificationsContact);

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.NotificationsContactId));

            if (criteria.MessageTypes != null)
                query = query.Where(c => criteria.MessageTypes.Any(m => c.BaseNotificationsContact!.SubscribedNotifications.HasFlag(m)));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.BaseNotificationsContact!.MerchantId));

            if (criteria.PrinterWorkerIds != null)
                query = query.Where(q => criteria.PrinterWorkerIds.Contains(q.PrinterWorkerId));

            if (criteria.LocationIds != null)
                query = query.Where(q => q.LocationId.HasValue && criteria.LocationIds.Contains(q.LocationId.Value));

            if (criteria.IsDeleted.HasValue)
                query = query.Where(q => q.DeletedDate.HasValue == criteria.IsDeleted.Value);

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}