using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Configurations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Storage;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MenuItemMapperHandler : IMapperHandler<MenuItem, Dtos.MenuItem>
    {
        private readonly IIdConverter idConverter;
        private readonly IDefaultSettings defaultSettings;

        public MenuItemMapperHandler(IIdConverter idConverter, IDefaultSettings defaultSettings)
        {
            this.idConverter = idConverter;
            this.defaultSettings = defaultSettings;
        }

        public Dtos.MenuItem Map(MenuItem model)
        {
            return new Dtos.MenuItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? defaultSettings.DefaultMerchantLogo : model.ImageUrl.Replace(ImageSize.Full.ToString(), ImageSize.Thumbnail.ToString()),
                Price = model.Price,
                ModifierGroups = [],
            };
        }
    }
}