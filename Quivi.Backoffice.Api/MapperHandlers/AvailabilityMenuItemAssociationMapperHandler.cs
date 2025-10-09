using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class AvailabilityMenuItemAssociationMapperHandler : IMapperHandler<AvailabilityMenuItemAssociation, Dtos.AvailabilityMenuItemAssociation>
    {
        private readonly IIdConverter idConverter;

        public AvailabilityMenuItemAssociationMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.AvailabilityMenuItemAssociation Map(AvailabilityMenuItemAssociation model)
        {
            return new Dtos.AvailabilityMenuItemAssociation
            {
                AvailabilityId = idConverter.ToPublicId(model.AvailabilityGroupId),
                MenuItemId = idConverter.ToPublicId(model.MenuItemId),
            };
        }
    }
}