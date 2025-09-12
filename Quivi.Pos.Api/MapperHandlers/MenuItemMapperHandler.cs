using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MenuItemMapperHandler : IMapperHandler<MenuItem, Dtos.MenuItem>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public MenuItemMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Dtos.MenuItem Map(MenuItem model)
        {
            return new Dtos.MenuItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
                Price = model.Price,
                ModifierGroups = mapper.Map<Dtos.ModifierGroup>((model.MenuItemModifierGroups ?? []).OrderBy(o => o.SortIndex).Select(o => o.MenuItemModifierGroup!)),
                HasStock = model.Stock,
                IsDeleted = model.DeletedDate.HasValue,
            };
        }
    }
}