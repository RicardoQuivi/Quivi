using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class OrderConfigurableFieldChannelProfileAssociationMapperHandler : IMapperHandler<OrderConfigurableFieldChannelProfileAssociation, ConfigurableFieldAssociation>
    {
        private readonly IIdConverter idConverter;

        public OrderConfigurableFieldChannelProfileAssociationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public ConfigurableFieldAssociation Map(OrderConfigurableFieldChannelProfileAssociation model)
        {
            return new ConfigurableFieldAssociation
            {
                ChannelProfileId = idConverter.ToPublicId(model.ChannelProfileId),
                ConfigurableFieldId = idConverter.ToPublicId(model.OrderConfigurableFieldId),
            };
        }
    }
}