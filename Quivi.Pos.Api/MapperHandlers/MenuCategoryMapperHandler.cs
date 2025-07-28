using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;
using Quivi.Pos.Api.Dtos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MenuCategoryMapperHandler : IMapperHandler<ItemCategory, Dtos.MenuCategory>
    {
        private readonly IIdConverter idConverter;

        public MenuCategoryMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public MenuCategory Map(ItemCategory model)
        {
            return new MenuCategory
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                ImageUrl = model.ImagePath?.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
            };
        }
    }
}
