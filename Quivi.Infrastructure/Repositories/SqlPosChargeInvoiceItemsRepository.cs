using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosChargeInvoiceItemsRepository : ARepository<PosChargeInvoiceItem, GetPosChargeInvoiceItemCriteria>, IPosChargeInvoiceItemsRepository
    {
        public SqlPosChargeInvoiceItemsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PosChargeInvoiceItem> GetFilteredQueryable(GetPosChargeInvoiceItemCriteria criteria)
        {
            IQueryable<PosChargeInvoiceItem> query = Set;

            if (criteria.IncludeOrderMenuItem)
                query = query.Include(q => q.OrderMenuItem);

            if (criteria.IncludePosChargeChargeInvoiceDocuments)
                query = query.Include(q => q.PosCharge!)
                            .ThenInclude(q => q.Charge!)
                            .ThenInclude(q => q.InvoiceDocuments);

            if (criteria.IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod)
                query = query.Include(q => q.PosCharge!)
                            .ThenInclude(q => q.Charge!)
                            .ThenInclude(q => q.MerchantCustomCharge!)
                            .ThenInclude(q => q.CustomChargeMethod!);

            if (criteria.IncludeChildrenPosChargeInvoiceItems)
            {
                query = query.Include(c => c.ChildrenPosChargeInvoiceItems);

                if (criteria.IncludeOrderMenuItem)
                    query = query.Include(c => c.ChildrenPosChargeInvoiceItems!).ThenInclude(c => c.OrderMenuItem);
            }

            if (criteria.ParentMerchantIds != null)
                query = query.Where(c => criteria.ParentMerchantIds.Contains(c.PosCharge!.Merchant!.ParentMerchantId!.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(c => criteria.MerchantIds.Contains(c.PosCharge!.MerchantId));

            if (criteria.PosChargeIds != null)
                query = query.Where(c => criteria.PosChargeIds.Contains(c.PosChargeId));

            if (criteria.FromDate.HasValue || criteria.ToDate.HasValue)
            {
                query = query.Where(c => c.PosCharge!.CaptureDate.HasValue);

                if (criteria.FromDate.HasValue)
                    query = query.Where(c => criteria.FromDate.Value <= c.PosCharge!.CaptureDate!.Value);

                if (criteria.ToDate.HasValue)
                    query = query.Where(c => c.PosCharge!.CaptureDate <= criteria.ToDate.Value);
            }

            if (criteria.IsParent.HasValue)
                query = query.Where(c => !c.ParentPosChargeInvoiceItemId.HasValue == criteria.IsParent.Value);

            return query.OrderBy(q => q.OrderMenuItemId);
        }
    }
}