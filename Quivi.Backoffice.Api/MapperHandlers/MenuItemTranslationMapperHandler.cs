using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MenuItemTranslationMapperHandler : IMapperHandler<MenuItemTranslation, Dtos.MenuItemTranslation>
    {
        public Dtos.MenuItemTranslation Map(MenuItemTranslation model)
        {
            return new Dtos.MenuItemTranslation
            {
                Name = model.Name,
                Description = model.Description,
            };
        }
    }
}
