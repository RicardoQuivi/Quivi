using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosChargesRepository : ARepository<PosCharge, GetPosChargesCriteria>, IPosChargesRepository
    {
        public SqlPosChargesRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<PosCharge> GetFilteredQueryable(GetPosChargesCriteria criteria)
        {
            IQueryable<PosCharge> query = Set;

            if (criteria.IncludePosChargeSelectedMenuItems)
                query = query.Include(q => q.PosChargeSelectedMenuItems);

            if (criteria.IncludePosChargeInvoiceItems)
                query = query.Include(q => q.PosChargeInvoiceItems);

            if (criteria.IncludePosChargeInvoiceItemsOrderMenuItems)
                query = query.Include(q => q.PosChargeInvoiceItems!).ThenInclude(q => q.OrderMenuItem);

            if (criteria.IncludeMerchant)
                query = query.Include(q => q.Merchant);

            if (criteria.IncludeCharge)
                query = query.Include(q => q.Charge);

            if (criteria.IncludePosChargeSyncAttempts)
                query = query.Include(q => q.PosChargeSyncAttempts);

            if (criteria.IncludeMerchantCustomCharge)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.MerchantCustomCharge!);

            if (criteria.IncludeReview)
                query = query.Include(q => q.Review);

            if (criteria.ParentMerchantIds != null)
                query = query.Where(q => q.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(q.Merchant!.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                query = query.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Ids != null)
                query = query.Where(q => criteria.Ids.Contains(q.ChargeId));

            if (criteria.SessionIds != null)
                query = query.Where(q => q.SessionId.HasValue && criteria.SessionIds.Contains(q.SessionId.Value));

            if (criteria.ChannelIds != null)
                query = query.Where(q => criteria.ChannelIds.Contains(q.ChannelId));

            if (criteria.IsCaptured.HasValue)
                query = query.Where(q => q.CaptureDate.HasValue == criteria.IsCaptured.Value);

            if(criteria.OrderIds != null)
            {
                var sessionIds = Context.Orders.Where(o => criteria.OrderIds.Contains(o.Id))
                                                .Where(o => o.SessionId.HasValue)
                                                .Select(o => o.SessionId!.Value)
                                                .Distinct();

                query = query.Where(q => (q.SessionId.HasValue && sessionIds.Contains(q.SessionId.Value)) ||
                                            q.PosChargeSelectedMenuItems!.Select(q => q.OrderMenuItem!).All(omi => criteria.OrderIds.Contains(omi.OrderId)));
            }

            if (criteria.FromCapturedDate.HasValue)
                query = query.Where(q => criteria.FromCapturedDate.Value <= q.CaptureDate);

            if (criteria.ToCapturedDate.HasValue)
                query = query.Where(q => q.CaptureDate <= criteria.ToCapturedDate.Value);

            if (criteria.HasSession.HasValue)
                query = query.Where(q => q.SessionId.HasValue == criteria.HasSession.Value);

            if (criteria.HasDiscounts.HasValue)
                query = query.Where(q => q.PosChargeInvoiceItems!.Any(item => item.OrderMenuItem!.OriginalPrice > item.OrderMenuItem!.FinalPrice) == criteria.HasDiscounts.Value);

            if (criteria.HasReview.HasValue)
                query = query.Where(q => q.Review != null);

            if (criteria.HasReviewComment.HasValue)
                query = query.Where(q => q.Review != null && string.IsNullOrEmpty(q.Review.Comment) == criteria.HasReviewComment.Value);

            if (criteria.CustomChargeMethodIds != null)
                query = query.Where(r => criteria.CustomChargeMethodIds.Contains(r.Charge!.MerchantCustomCharge!.CustomChargeMethodId));

            if (criteria.HasRefunds.HasValue)
                query = criteria.HasRefunds.Value ? query.Where(r => r.TotalRefund > 0) : query.Where(r => r.TotalRefund == 0);

            if (criteria.QuiviPaymentsOnly.HasValue)
                query = query.Where(r => !(r.Charge!.ChargeMethod == ChargeMethod.Custom));

            if (criteria.SyncingState.HasValue)
            {
                if (criteria.SyncingState == SyncAttemptState.Syncing)
                    query = query.Where(q => q.PosChargeSyncAttempts!.Any(pcsa => pcsa.State == SyncAttemptState.Synced || pcsa.State == SyncAttemptState.Failed) == false);
                else
                    query = query.Where(q => q.PosChargeSyncAttempts!.Any(pcsa => pcsa.State == criteria.SyncingState.Value));
            }

            return query.OrderByDescending(o => o.CaptureDate.HasValue ? o.CaptureDate : o.CreatedDate);
        }
    }
}