using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Pos.Api.Dtos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PosChargeMapperHandler : IMapperHandler<PosCharge, Dtos.Transaction>
    {
        private readonly IIdConverter idConverter;

        public PosChargeMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Transaction Map(PosCharge model)
        {
            return new Transaction
            {
                Id = idConverter.ToPublicId(model.Id),
                IsSynced = model.PosChargeInvoiceItems?.Any() == true,
            };
        }
    }
}