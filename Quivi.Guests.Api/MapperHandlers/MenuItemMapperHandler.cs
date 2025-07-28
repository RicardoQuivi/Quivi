using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MenuItemMapperHandler : IMapperHandler<MenuItem, Dtos.MenuItem>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MenuItemMapperHandler(IIdConverter idConverter, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Dtos.MenuItem Map(MenuItem model)
        {
            var translation = GetTranslations(model);
            return new Dtos.MenuItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = string.IsNullOrWhiteSpace(translation?.Name) ? model.Name : translation.Name,
                Description = string.IsNullOrWhiteSpace(translation?.Description) ? model.Description : translation.Description,
                Price = model.Price,
                PriceType = model.PriceType.ToString(),
                ImageUrl = model.ImageUrl?.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
                Modifiers = mapper.Map<Dtos.MenuItemModifierGroup>(model.MenuItemModifierGroups?.OrderBy(r => r.SortIndex).Select(r => r.MenuItemModifierGroup).ToList() ?? []),
                IsAvailable = model.IsUnavailable || model.Stock == false,
            };
        }

        private MenuItemTranslation? GetTranslations(MenuItem model)
        {
            var rawLanguage = httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"];
            var language = rawLanguage.HasValue ? mapper.Map<string, Language?>(rawLanguage.Value.ToString()) : null;
            var translation = language.HasValue ? model.MenuItemTranslations?.Where(t => t.DeletedDate.HasValue == false)
                                                                                .SingleOrDefault(t => t.Language == language.Value) : null;

            return translation;
        }
    }
}