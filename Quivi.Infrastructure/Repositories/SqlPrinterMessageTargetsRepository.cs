using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPrinterMessageTargetsRepository : ARepository<PrinterMessageTarget, GetPrinterMessageTargetsCriteria>, IPrinterMessageTargetsRepository
    {
        public SqlPrinterMessageTargetsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PrinterMessageTarget> GetFilteredQueryable(GetPrinterMessageTargetsCriteria criteria)
        {
            IQueryable<PrinterMessageTarget> query = Set;

            if(criteria.IncludePrinterNotificationMessage)
                query = query.Include(q => q.PrinterNotificationMessage);

            if (criteria.IncludePrinterNotificationsContactPrinterWorker)
                query = query.Include(q => q.PrinterNotificationsContact).ThenInclude(q => q.PrinterWorker);

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.PrinterNotificationMessage!.MerchantId));

            if (criteria.PrinterNotificationMessageIds != null)
                query = query.Where(q => criteria.PrinterNotificationMessageIds.Contains(q.PrinterNotificationMessageId));

            if (criteria.PrinterNotificationsContactIds != null)
                query = query.Where(q => criteria.PrinterNotificationsContactIds.Contains(q.PrinterNotificationsContactId));

            if (criteria.DeletedTargets != null)
                query = query.Where(q => q.PrinterNotificationsContact!.DeletedDate.HasValue ==  criteria.DeletedTargets.Value);

            return query.OrderByDescending(q => q.ModifiedDate);
        }
    }
}