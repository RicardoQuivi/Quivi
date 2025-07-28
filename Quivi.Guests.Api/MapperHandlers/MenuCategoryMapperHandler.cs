using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MenuCategoryMapperHandler : IMapperHandler<ItemCategory, MenuCategory>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MenuCategoryMapperHandler(IIdConverter idConverter, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public MenuCategory Map(ItemCategory model)
        {
            var translation = GetTranslations(model);
            return new MenuCategory
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = string.IsNullOrWhiteSpace(translation?.Name) ? model.Name : translation.Name,
                ImageUrl = model.ImagePath?.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
            };
        }

        private ItemCategoryTranslation? GetTranslations(ItemCategory model)
        {
            var rawLanguage = httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"];
            var language = rawLanguage.HasValue ? mapper.Map<string, Language?>(rawLanguage.Value.ToString()) : null;
            var translation = language.HasValue ? model.ItemCategoryTranslations?.Where(t => t.DeletedDate.HasValue == false)
                                                                                .SingleOrDefault(t => t.Language == language.Value) : null;

            return translation;
        }
    }
}