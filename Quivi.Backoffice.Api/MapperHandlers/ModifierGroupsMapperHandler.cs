using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ModifierGroupsMapperHandler : IMapperHandler<ItemsModifierGroup, Dtos.ModifierGroup>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ModifierGroupsMapperHandler(IIdConverter idConverter, IMapper mapper)
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
                Items = model.MenuItemModifiers?.ToDictionary(s => idConverter.ToPublicId(s.MenuItemId), mapper.Map<Dtos.ModifierGroupItem>) ?? [],
                Translations = model.ItemsModifierGroupTranslations?.ToDictionary(s => s.Language, mapper.Map<Dtos.ModifierGroupTranslation>) ?? [],
            };
        }
    }
}
