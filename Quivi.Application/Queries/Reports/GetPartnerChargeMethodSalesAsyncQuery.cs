using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Repositories;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Repositories.Data;
using Quivi.Infrastructure.Cqrs;

namespace Quivi.Application.Queries.Reports
{
    public class GetPartnerChargeMethodSalesAsyncQuery : APagedAsyncQuery<Domain.Repositories.EntityFramework.Models.PartnerChargeMethodSales>
    {
        public IEnumerable<ChargePartner>? ChargePartners { get; set; }
        public IEnumerable<ChargeMethod>? ChargeMethods { get; set; }
        public SalesPeriod? Period { get; init; }
        public IEnumerable<int>? ParentMerchantIds { get; init; }
        public IEnumerable<int>? MerchantIds { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
    }

    internal class GetPartnerChargeMethodSalesAsyncQueryHandler : APagedQueryAsyncHandler<GetPartnerChargeMethodSalesAsyncQuery, Domain.Repositories.EntityFramework.Models.PartnerChargeMethodSales>
    {
        private readonly IReportsRepository repository;

        public GetPartnerChargeMethodSalesAsyncQueryHandler(IReportsRepository repository)
        {
            this.repository = repository;
        }

        public override Task<IPagedData<Domain.Repositories.EntityFramework.Models.PartnerChargeMethodSales>> Handle(GetPartnerChargeMethodSalesAsyncQuery query)
        {
            return repository.GetPartnerChargeMethodSalesAsync(new GetPartnerChargeMethodSalesCriteria
            {
                ChargeMethods = query.ChargeMethods,
                ChargePartners = query.ChargePartners,
                Period = query.Period,
                ParentMerchantIds = query.ParentMerchantIds,
                MerchantIds = query.MerchantIds,
                From = query.From,
                To = query.To,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
            });
        }
    }
}