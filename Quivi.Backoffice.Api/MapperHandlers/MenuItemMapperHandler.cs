using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MenuItemMapperHandler : IMapperHandler<MenuItem, Dtos.MenuItem>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public MenuItemMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public Dtos.MenuItem Map(MenuItem model)
        {
            return new Dtos.MenuItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.ImageUrl?.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
                Price = model.Price,
                PriceType = model.PriceType,
                LocationId = model.LocationId.HasValue ? idConverter.ToPublicId(model.LocationId.Value) : null,
                ShowWhenNotAvailable = model.ShowWhenNotAvailable,
                SortIndex = model.SortIndex,
                Stock = model.Stock,
                VatRate = model.VatRate,
                MenuCategoryIds = model.MenuItemCategoryAssociations?.Select(s => idConverter.ToPublicId(s.ItemCategoryId)) ?? [],
                ModifierGroupIds = model.MenuItemModifierGroups?.Select(s => idConverter.ToPublicId(s.MenuItemModifierGroupId)) ?? [],
                Translations = model.MenuItemTranslations?.ToDictionary(s => s.Language, mapper.Map<Dtos.MenuItemTranslation>) ?? [],
            };
        }
    }
}