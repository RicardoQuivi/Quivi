using Microsoft.EntityFrameworkCore;
using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Repositories.EntityFramework;
using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Infrastructure.Repositories
{
    public class SqlSettlementsRepository : ARepository<Settlement, GetSettlementsCriteria>, ISettlementsRepository
    {
        public SqlSettlementsRepository(QuiviContext context) : base(context)
        {
        }

        public override IOrderedQueryable<Settlement> GetFilteredQueryable(GetSettlementsCriteria criteria)
        {
            IQueryable<Settlement> query = Set;

            if (criteria.IncludeSettlementDetails)
                query = query.Include(q => q.SettlementDetails);

            if (criteria.IncludeSettlementServiceDetails)
                query = query.Include(q => q.SettlementServiceDetails);

            if (criteria.States != null)
                query = query.Where(q => criteria.States.Contains(q.State));

            if (criteria.Dates != null)
            {
                var dates = criteria.Dates.Select(s => s.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToList();
                query = query.Where(q => dates.Contains(q.Date));
            }

            if (criteria.ParentMerchantIds != null)
                query = query.Where(q => q.SettlementDetails!.Any(s => criteria.ParentMerchantIds.Contains(s.ParentMerchantId)));

            if (criteria.MerchantIds != null)
                query = query.Where(q => q.SettlementDetails!.Any(s => criteria.MerchantIds.Contains(s.MerchantId)));

            return query.OrderByDescending(q => q.Date);
        }

        public async Task<IPagedData<MerchantSettlementResume>> GetMerchantSettlementResumes(GetMerchantSettlementResumesCriteria criteria)
        {
            var query = Set.Where(r => r.State == SettlementState.Finished);

            if (criteria.SettlementIds != null)
                query = query.Where(q => criteria.SettlementIds.Contains(q.Id));

            if (criteria.Dates != null)
            {
                var dates = criteria.Dates.Select(s => s.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToList();
                query = query.Where(q => dates.Contains(q.Date));
            }

            if (criteria.ParentMerchantIds != null || criteria.MerchantIds != null || criteria.ChargeMethods != null)
            {

                if (criteria.ParentMerchantIds != null)
                    query = query.Where(q => q.SettlementDetails!.Any(s => criteria.ParentMerchantIds.Contains(s.ParentMerchantId)));

                if (criteria.MerchantIds != null)
                    query = query.Where(q => q.SettlementDetails!.Any(s => criteria.MerchantIds.Contains(s.MerchantId)));

                if (criteria.ChargeMethods != null)
                    query = query.Where(q => q.SettlementDetails!.Any(s => criteria.ChargeMethods.Contains(s.Journal!.ChargeMethod!.Value)));

                query = query.Include(s => s.SettlementDetails!.Where(q =>
                                                                            (criteria.ParentMerchantIds == null || criteria.ParentMerchantIds.Contains(q.ParentMerchantId)) &&
                                                                            (criteria.MerchantIds == null || criteria.MerchantIds.Contains(q.MerchantId)) &&
                                                                            (criteria.ChargeMethods == null || criteria.ChargeMethods.Contains(q.Journal!.ChargeMethod!.Value))
                                                                      ))
                                .Include(s => s.SettlementServiceDetails!.Where(q =>
                                                                                    (criteria.ParentMerchantIds == null || criteria.ParentMerchantIds.Contains(q.ParentMerchantId)) &&
                                                                                    (criteria.MerchantIds == null || criteria.MerchantIds.Contains(q.MerchantId)) &&
                                                                                    (criteria.ChargeMethods == null)
                                                                                ));
            }

            var aux = await query.ToListAsync();

            var unorderedQuery = query.SelectMany(r => r.SettlementDetails!)
                    .GroupBy(r => new { r.Settlement!.Id, r.Settlement!.Date, r.MerchantId, r.ParentMerchantId })
                    .Select(x => new
                    {
                        Id = x.Key.Id,
                        Date = x.Key.Date,

                        ParentMerchantId = x.Key.ParentMerchantId,
                        MerchantId = x.Key.MerchantId,

                        IncludedNetTip = x.Sum(y => y.IncludedNetTip),
                        IncludedTip = x.Sum(y => y.IncludedTip),
                        NetAmount = x.Sum(y => y.NetAmount),
                        Amount = x.Sum(y => y.Amount),
                        ServiceAmount = 0.0M,
                        ServiceVatAmount = 0.0M,
                    }).Union(query.SelectMany(r => r.SettlementDetails!)
                                    .GroupBy(r => new { r.Settlement!.Id, r.Settlement!.Date, r.MerchantId, r.ParentMerchantId })
                                    .Select(x => new
                                    {
                                        Id = x.Key.Id,
                                        Date = x.Key.Date,

                                        ParentMerchantId = x.Key.ParentMerchantId,
                                        MerchantId = x.Key.MerchantId,

                                        IncludedNetTip = 0.0M,
                                        IncludedTip = 0.0M,
                                        NetAmount = 0.0M,
                                        Amount = 0.0M,
                                        ServiceAmount = x.Sum(y => y.Amount),
                                        ServiceVatAmount = x.Sum(y => y.VatAmount),
                                    })
                    ).GroupBy(r => new { r.Id, r.Date, r.MerchantId, r.ParentMerchantId })
                    .Select(a => new
                    {
                        Id = a.Key.Id,
                        Date = a.Key.Date,

                        ParentMerchantId = a.Key.ParentMerchantId,
                        MerchantId = a.Key.MerchantId,

                        IncludedNetTip = a.Sum(x => x.IncludedNetTip),
                        IncludedTip = a.Sum(x => x.IncludedTip),
                        NetAmount = a.Sum(x => x.NetAmount),
                        Amount = a.Sum(x => x.Amount),
                        ServiceAmount = a.Sum(x => x.ServiceAmount),
                        ServiceVatAmount = a.Sum(x => x.ServiceVatAmount),
                    });

            var queryResult = await unorderedQuery.OrderByDescending(r => r.Date).ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            var result = queryResult.Select(s => new MerchantSettlementResume
            {
                Id = s.Id,
                Date = s.Date,

                ParentMerchantId = s.ParentMerchantId,
                MerchantId = s.MerchantId,

                GrossAmount = s.Amount - s.IncludedTip,
                NetAmount = s.NetAmount - s.IncludedNetTip,
                GrossTip = s.IncludedTip,
                NetTip = s.IncludedNetTip,
                ServiceAmount = s.ServiceAmount + s.ServiceVatAmount,
            });

            return new PagedData<MerchantSettlementResume>(result)
            {
                CurrentPage = queryResult.CurrentPage,
                NumberOfPages = queryResult.NumberOfPages,
                TotalItems = queryResult.TotalItems,
            };
        }

        public async Task<IPagedData<MerchantSettlementDetail>> GetMerchantSettlementDetails(GetMerchantSettlementDetailsCriteria criteria)
        {
            var query = Set.Where(r => r.State == SettlementState.Finished).SelectMany(q => q.SettlementDetails!);

            if (criteria.SettlementIds != null)
                query = query.Where(q => criteria.SettlementIds.Contains(q.SettlementId));

            if (criteria.ChargeMethods != null)
                query = query.Where(q => criteria.ChargeMethods.Contains(q.Journal!.ChargeMethod!.Value));

            if (criteria.ParentMerchantIds != null)
                query = query.Where(s => criteria.ParentMerchantIds.Contains(s.ParentMerchantId));

            if (criteria.MerchantIds != null)
                query = query.Where(s => criteria.MerchantIds.Contains(s.MerchantId));

            var preResult = await query.Select(q => new
            {
                q.JournalId,
                q.SettlementId,

                q.MerchantId,
                q.ParentMerchantId,

                q.Amount,
                q.IncludedTip,

                q.NetAmount,
                q.IncludedNetTip,

                TransactionDate = q.Journal!.CreatedDate,
            }).OrderByDescending(q => q.TransactionDate).ToPagedDataAsync(criteria.PageIndex, criteria.PageSize);

            return new PagedData<MerchantSettlementDetail>(preResult.Select(q => new MerchantSettlementDetail
            {
                JournalId = q.JournalId,
                SettlementId = q.SettlementId,

                TransactionDate = q.TransactionDate,

                MerchantId = q.MerchantId,
                ParentMerchantId = q.ParentMerchantId,

                GrossAmount = q.Amount - q.IncludedTip,
                GrossTip = q.IncludedTip,

                NetAmount = q.NetAmount - q.IncludedNetTip,
                NetTip = q.IncludedTip,
            }))
            {
                CurrentPage = preResult.CurrentPage,
                NumberOfPages = preResult.NumberOfPages,
                TotalItems = preResult.TotalItems,
            };
        }
    }
}