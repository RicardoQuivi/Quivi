using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AvailabilityProfileAssociationMapperHandler : IMapperHandler<AvailabilityProfileAssociation, Dtos.AvailabilityChannelProfileAssociation>
    {
        private readonly IIdConverter idConverter;

        public AvailabilityProfileAssociationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.AvailabilityChannelProfileAssociation Map(AvailabilityProfileAssociation model)
        {
            return new Dtos.AvailabilityChannelProfileAssociation
            {
                AvailabilityId = idConverter.ToPublicId(model.AvailabilityGroupId),
                ChannelProfileId = idConverter.ToPublicId(model.ChannelProfileId),
            };
        }
    }
}