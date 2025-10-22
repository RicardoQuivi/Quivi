using Quivi.Domain.Repositories.EntityFramework.Models;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MerchantSettlementDetailMapperHandler : IMapperHandler<MerchantSettlementDetail, Dtos.SettlementDetail>
    {
        private readonly IIdConverter idConverter;

        public MerchantSettlementDetailMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.SettlementDetail Map(MerchantSettlementDetail model)
        {
            return new Dtos.SettlementDetail
            {
                Id = idConverter.ToPublicId(model.JournalId),
                SettlementId = idConverter.ToPublicId(model.SettlementId),
                Date = new DateTimeOffset(model.TransactionDate, TimeSpan.Zero),

                ParentMerchantId = idConverter.ToPublicId(model.ParentMerchantId),
                MerchantId = idConverter.ToPublicId(model.MerchantId),

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