using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MerchantSettlementResumeMapperHandler : IMapperHandler<MerchantSettlementResume, Dtos.Settlement>
    {
        private readonly IIdConverter idConverter;

        public MerchantSettlementResumeMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.Settlement Map(MerchantSettlementResume model)
        {
            return new Dtos.Settlement
            {
                Id = idConverter.ToPublicId(model.Id),
                Date = DateOnly.FromDateTime(model.Date),

                ServiceAmount = model.ServiceAmount,

                GrossAmount = model.GrossAmount,
                GrossTip = model.GrossTip,
                GrossTotal = model.GrossTotal,

                NetAmount = model.NetAmount,
                NetTip = model.NetTip,
                NetTotal = model.NetTotal,
            };
        }
    }
}