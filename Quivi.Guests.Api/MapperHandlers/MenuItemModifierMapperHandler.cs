using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MenuItemModifierMapperHandler : IMapperHandler<MenuItemModifier, Dtos.MenuItem>
    {
        private readonly IMapper mapper;

        public MenuItemModifierMapperHandler(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public Dtos.MenuItem Map(MenuItemModifier model)
        {
            if (model.MenuItem == null)
                throw new ArgumentException($"{nameof(MenuItemModifier)} must have a {nameof(model.MenuItem)} associated to be mapped.");

            var originalPrice = model.MenuItem.Price;
            model.MenuItem.Price = model.Price;

            //Hammer fix: Clear temporarily modifiers (only one level of modifiers are allowed)
            //This avoids StackOverflowException if, by mistake, some Item has modifiers that, somehow,
            //references itself. Again, this fix will always be valid as long as we only support one level of modifiers.
            var originalModifiers = model.MenuItem.MenuItemModifierGroups;
            model.MenuItem.MenuItemModifierGroups = null;

            var result = mapper.Map<Dtos.MenuItem>(model.MenuItem);

            //Restore original values
            model.MenuItem.MenuItemModifierGroups = originalModifiers;
            model.MenuItem.Price = originalPrice;

            return result;
        }
    }
}
