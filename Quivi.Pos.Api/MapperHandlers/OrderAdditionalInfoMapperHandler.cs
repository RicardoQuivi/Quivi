using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Pos.Api.Dtos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class OrderAdditionalInfoMapperHandler : IMapperHandler<OrderAdditionalInfo, Dtos.SessionAdditionalInfo>
    {
        private readonly IIdConverter idConverter;

        public OrderAdditionalInfoMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }


        public SessionAdditionalInfo Map(OrderAdditionalInfo model)
        {
            return new SessionAdditionalInfo
            {
                Id = idConverter.ToPublicId(model.OrderConfigurableFieldId),
                Value = model.Value,
                OrderId = idConverter.ToPublicId(model.OrderId),
            };
        }
    }
}