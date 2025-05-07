using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MenuCategoryTranslationMapperHandler : IMapperHandler<ItemCategoryTranslation, MenuCategoryTranslation>
    {
        public MenuCategoryTranslation Map(ItemCategoryTranslation model)
        {
            return new MenuCategoryTranslation
            {
                Name = model.Name,
            };
        }
    }
}
