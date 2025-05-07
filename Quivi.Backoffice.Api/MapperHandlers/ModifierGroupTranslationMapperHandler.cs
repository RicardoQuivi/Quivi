using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class ModifierGroupTranslationMapperHandler : IMapperHandler<ItemsModifierGroupTranslation, Dtos.ModifierGroupTranslation>
    {
        public Dtos.ModifierGroupTranslation Map(ItemsModifierGroupTranslation model)
        {
            return new Dtos.ModifierGroupTranslation
            {
                Name = model.Name,
            };
        }
    }
}
