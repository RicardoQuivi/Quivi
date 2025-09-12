using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class ItemsModifierGroupsAssociationMapperHandler : IMapperHandler<ItemsModifierGroup, Dtos.ModifierGroup>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ItemsModifierGroupsAssociationMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.ModifierGroup Map(ItemsModifierGroup model)
        {
            return new Dtos.ModifierGroup
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                MinSelection = model.MinSelection,
                MaxSelection = model.MaxSelection,
                Options = mapper.Map<Dtos.ModifierGroupOption>(model.MenuItemModifiers!)!,
            };
        }
    }
}