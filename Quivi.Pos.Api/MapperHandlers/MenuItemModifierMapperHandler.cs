using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MenuItemModifierMapperHandler : IMapperHandler<MenuItemModifier, Dtos.ModifierGroupOption>
    {
        private readonly IIdConverter idConverter;

        public MenuItemModifierMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.ModifierGroupOption Map(MenuItemModifier model)
        {
            return new Dtos.ModifierGroupOption
            {
                Id = idConverter.ToPublicId(model.Id),
                MenuItemId = idConverter.ToPublicId(model.MenuItemId),
                Price = model.Price,
            };
        }
    }
}