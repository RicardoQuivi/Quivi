using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PartnerChargeMethodSalesMapperHandler : IMapperHandler<PartnerChargeMethodSales, Dtos.PartnerChargeMethodSales>
    {
        public Dtos.PartnerChargeMethodSales Map(PartnerChargeMethodSales model)
        {
            return new Dtos.PartnerChargeMethodSales
            {
                ChargeMethod = model.ChargeMethod,
                ChargePartner = model.ChargePartner,

                From = new DateTimeOffset(model.From, TimeSpan.Zero),
                To = new DateTimeOffset(model.To, TimeSpan.Zero),

                Total = model.TotalSuccess + model.TotalFailed + model.TotalProcessing,
                TotalSuccess = model.TotalSuccess,
                TotalFailed = model.TotalFailed,
                TotalProcessing = model.TotalProcessing,
            };
        }
    }
}