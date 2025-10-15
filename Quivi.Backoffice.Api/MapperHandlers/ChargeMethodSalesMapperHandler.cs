using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ChargeMethodSalesMapperHandler : IMapperHandler<ChargeMethodSales, Dtos.ChargeMethodSales>
    {
        private readonly IIdConverter idConverter;

        public ChargeMethodSalesMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.ChargeMethodSales Map(ChargeMethodSales model)
        {
            return new Dtos.ChargeMethodSales
            {
                CustomChargeMethodId = model.CustomChargeMethodId.HasValue ? idConverter.ToPublicId(model.CustomChargeMethodId.Value) : null,

                From = new DateTimeOffset(model.From, TimeSpan.Zero),
                To = new DateTimeOffset(model.To, TimeSpan.Zero),

                TotalInvoices = model.TotalQuantity,
                TotalBilledAmount = model.TotalBilledAmount,
            };
        }
    }
}