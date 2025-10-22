using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using System.Linq.Expressions;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlPosChargesRepository : ARepository<PosCharge, GetPosChargesCriteria>, IPosChargesRepository
    {
        public SqlPosChargesRepository(QuiviContext context) : base(context)
        {
        }

        private IQueryable<PosCharge> Filter(IQueryable<PosCharge> query, AGetPosChargesCriteria criteria)
        {
            var innerQuery = query;
            if (criteria.ParentMerchantIds != null)
                innerQuery = innerQuery.Where(q => q.Merchant!.ParentMerchantId.HasValue && criteria.ParentMerchantIds.Contains(q.Merchant!.ParentMerchantId.Value));

            if (criteria.MerchantIds != null)
                innerQuery = innerQuery.Where(q => criteria.MerchantIds.Contains(q.MerchantId));

            if (criteria.Ids != null)
                innerQuery = innerQuery.Where(q => criteria.Ids.Contains(q.ChargeId));

            if (criteria.SessionIds != null)
                innerQuery = innerQuery.Where(q => q.SessionId.HasValue && criteria.SessionIds.Contains(q.SessionId.Value));

            if (criteria.ChannelIds != null)
                innerQuery = innerQuery.Where(q => criteria.ChannelIds.Contains(q.ChannelId));

            if (criteria.IsCaptured.HasValue)
                innerQuery = innerQuery.Where(q => q.CaptureDate.HasValue == criteria.IsCaptured.Value);

            if (criteria.OrderIds != null)
            {
                //var sessionIds = Context.Orders.Where(o => criteria.OrderIds.Contains(o.Id))
                //                                .Where(o => o.SessionId.HasValue)
                //                                .Select(o => o.SessionId!.Value)
                //                                .Distinct();

                innerQuery = innerQuery.Where(q => //(q.SessionId.HasValue && sessionIds.Contains(q.SessionId.Value)) ||
                                            (
                                                q.PosChargeSelectedMenuItems!.Select(q => q.OrderMenuItem!).Any() &&
                                                q.PosChargeSelectedMenuItems!.Select(q => q.OrderMenuItem!).All(omi => criteria.OrderIds.Contains(omi.OrderId))
                                            ));
            }


            if (criteria.LocationIds != null)
                innerQuery = innerQuery.Where(q => q.LocationId.HasValue && criteria.LocationIds.Contains(q.LocationId.Value));

            if (criteria.FromCapturedDate.HasValue)
                innerQuery = innerQuery.Where(q => criteria.FromCapturedDate.Value <= q.CaptureDate);

            if (criteria.ToCapturedDate.HasValue)
                innerQuery = innerQuery.Where(q => q.CaptureDate <= criteria.ToCapturedDate.Value);

            if (criteria.HasSession.HasValue)
                innerQuery = innerQuery.Where(q => q.SessionId.HasValue == criteria.HasSession.Value);

            if (criteria.HasDiscounts.HasValue)
                innerQuery = innerQuery.Where(q => q.PosChargeInvoiceItems!.Any(item => item.OrderMenuItem!.OriginalPrice > item.OrderMenuItem!.FinalPrice) == criteria.HasDiscounts.Value);

            if (criteria.HasReview.HasValue)
                innerQuery = innerQuery.Where(q => q.Review != null);

            if (criteria.HasReviewComment.HasValue)
                innerQuery = innerQuery.Where(q => q.Review != null && string.IsNullOrEmpty(q.Review.Comment) == criteria.HasReviewComment.Value);

            if (criteria.CustomChargeMethodIds != null)
                innerQuery = innerQuery.Where(r => criteria.CustomChargeMethodIds.Contains(r.Charge!.MerchantCustomCharge!.CustomChargeMethodId));

            if (criteria.HasRefunds.HasValue)
                innerQuery = criteria.HasRefunds.Value ? innerQuery.Where(r => r.TotalRefund > 0) : innerQuery.Where(r => r.TotalRefund == 0);

            if (criteria.QuiviPaymentsOnly.HasValue)
                innerQuery = innerQuery.Where(r => !(r.Charge!.ChargeMethod == ChargeMethod.Custom));

            if (criteria.SyncingState.HasValue)
            {
                if (criteria.SyncingState == SyncAttemptState.Syncing)
                    innerQuery = innerQuery.Where(q => q.PosChargeSyncAttempts!.Any(pcsa => pcsa.State == SyncAttemptState.Synced || pcsa.State == SyncAttemptState.Failed) == false);
                else
                    innerQuery = innerQuery.Where(q => q.PosChargeSyncAttempts!.Any(pcsa => pcsa.State == criteria.SyncingState.Value));
            }

            return innerQuery;
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

            if (criteria.IncludeAcquirerCharge)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.AcquirerCharge);

            if (criteria.IncludePosChargeSyncAttempts)
                query = query.Include(q => q.PosChargeSyncAttempts);

            if (criteria.IncludeMerchantCustomCharge)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.MerchantCustomCharge!);

            if (criteria.IncludeMerchantCustomChargeCustomChargeMethod)
                query = query.Include(q => q.Charge!).ThenInclude(q => q.MerchantCustomCharge!).ThenInclude(q => q.CustomChargeMethod);

            if (criteria.IncludeReview)
                query = query.Include(q => q.Review);

            query = Filter(query, criteria);

            Expression<Func<PosCharge, DateTime>> sortExpression = (PosCharge o) => o.CaptureDate.HasValue ? o.CaptureDate.Value : o.CreatedDate;
            switch (criteria.SortDirection)
            {
                case Abstractions.Repositories.Data.SortDirection.Ascending: return query.OrderBy(sortExpression);
                case Abstractions.Repositories.Data.SortDirection.Descending: return query.OrderByDescending(sortExpression);
            }
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyDictionary<T, PosChargesResume>> GetResumeAsync<T>(GetPosChargesResumeCriteria criteria, Expression<Func<PosCharge, T>> grouping, T defaultKey)
        {
            IQueryable<PosCharge> query = Filter(Set, criteria);
            var preResult = query.GroupBy(grouping).Select(q => new
            {
                Key = q.Key,
                Resume = new PosChargesResume
                {
                    PaymentAmount = q.Sum(e => e.Payment) - q.Sum(e => e.PaymentRefund ?? 0.0M),
                    PaymentDiscount = q
                        .SelectMany(e => e.PosChargeInvoiceItems!)
                        .Sum(ii => (ii.ParentPosChargeInvoiceItemId.HasValue ? ii.ParentPosChargeInvoiceItem!.Quantity : 1) * ii.Quantity * (ii.ParentPosChargeInvoiceItem!.OrderMenuItem!.OriginalPrice - ii.ParentPosChargeInvoiceItem!.OrderMenuItem!.FinalPrice)),
                    SurchageAmount = q.Sum(e => e.SurchargeFeeAmount),
                    TipAmount = q.Sum(e => e.Tip) - q.Sum(e => e.TipRefund ?? 0.0M),
                    RefundedAmount = q.Sum(e => (e.PaymentRefund ?? 0.0M) + (e.TipRefund ?? 0.0M)),
                },
            });
            var listResult = await preResult.ToListAsync();
            return listResult.ToDictionary(t => t.Key == null ? defaultKey : t.Key, t => t.Resume);
        }
    }
}