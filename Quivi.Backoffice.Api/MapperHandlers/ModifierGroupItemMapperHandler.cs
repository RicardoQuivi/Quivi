using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ModifierGroupItemMapperHandler : IMapperHandler<MenuItemModifier, Dtos.ModifierGroupItem>
    {
        public Dtos.ModifierGroupItem Map(MenuItemModifier model)
        {
            return new Dtos.ModifierGroupItem
            {
                Price = model.Price,
                SortIndex = model.SortIndex,
            };
        }
    }
}
