using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPrinterNotificationMessagesRepository : ARepository<PrinterNotificationMessage, GetPrinterNotificationMessagesCriteria>, IPrinterNotificationMessagesRepository
    {
        public SqlPrinterNotificationMessagesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PrinterNotificationMessage> GetFilteredQueryable(GetPrinterNotificationMessagesCriteria criteria)
        {
            IQueryable<PrinterNotificationMessage> query = Set;

            if(criteria.IncludePrinterMessageTargets)
                query = query.Include(q => q.PrinterMessageTargets);

            if (criteria.IncludePrinterMessageTargetsPrinterNotificationsContact)
                query = query.Include(q => q.PrinterMessageTargets!).ThenInclude(q => q.PrinterNotificationsContact);

            if(criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.Id));

            return query.OrderBy(q => q.CreatedDate);
        }
    }
}