using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MenuCategoryMapperHandler : IMapperHandler<ItemCategory, Dtos.MenuCategory>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public MenuCategoryMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.MenuCategory Map(ItemCategory model)
        {
            return new Dtos.MenuCategory
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                ImageUrl = model.ImagePath,
                SortIndex = model.SortIndex,
                Translations = model.ItemCategoryTranslations?.ToDictionary(s => s.Language, mapper.Map<Dtos.MenuCategoryTranslation>) ?? [],
            };
        }
    }
}
