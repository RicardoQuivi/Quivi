using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MenuItemModifierGroupMapperHandler : IMapperHandler<ItemsModifierGroup, MenuItemModifierGroup>
    {
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MenuItemModifierGroupMapperHandler(IIdConverter idConverter, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public MenuItemModifierGroup Map(ItemsModifierGroup model)
        {
            var translation = GetTranslations(model);
            return new MenuItemModifierGroup
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = string.IsNullOrWhiteSpace(translation?.Name) ? model.Name : translation.Name,
                MinSelection = model.MinSelection,
                MaxSelection = model.MaxSelection,
                Options = mapper.Map<Dtos.MenuItem>(model.MenuItemModifiers?.OrderBy(r => r.SortIndex).ToList() ?? []),
            };
        }

        private ItemsModifierGroupTranslation? GetTranslations(ItemsModifierGroup model)
        {
            var rawLanguage = httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"];
            var language = rawLanguage.HasValue ? mapper.Map<string, Language?>(rawLanguage.Value.ToString()) : null;
            var translation = language.HasValue ? model.ItemsModifierGroupTranslations?.Where(t => t.DeletedDate.HasValue == false)
                                                                                .SingleOrDefault(t => t.Language == language.Value) : null;

            return translation;
        }
    }
}
