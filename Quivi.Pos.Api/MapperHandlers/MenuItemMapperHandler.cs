using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MenuItemMapperHandler : IMapperHandler<MenuItem, Dtos.MenuItem>
    {
        private readonly IIdConverter idConverter;

        public MenuItemMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.MenuItem Map(MenuItem model)
        {
            return new Dtos.MenuItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.Name,
                ImageUrl = model.ImageUrl,
                Price = model.Price,
                ModifierGroups = [],
            };
        }
    }
}
