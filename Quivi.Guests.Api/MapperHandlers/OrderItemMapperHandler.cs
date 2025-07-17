using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderItemMapperHandler : IMapperHandler<OrderMenuItem, Dtos.OrderItem>
    {
        private readonly IIdConverter idConverter;

        public OrderItemMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public Dtos.OrderItem Map(OrderMenuItem model)
        {
            return new Dtos.OrderItem
            {
                Id = idConverter.ToPublicId(model.MenuItemId),
                Name = model.Name,
                Amount = model.FinalPrice,
                Quantity = model.Quantity,
                Modifiers = model.Modifiers?.GroupBy(g => g.MenuItemModifierGroupId ?? 0).Select(m => new Dtos.ModifierGroup
                {
                    Id = idConverter.ToPublicId(m.Key),
                    SelectedOptions = m.Select(o => new Dtos.BaseOrderItem
                    {
                        Id = idConverter.ToPublicId(o.MenuItemId),
                        Name = o.Name,
                        Amount = o.FinalPrice,
                        Quantity = o.Quantity / model.Quantity,
                    }),
                }) ?? [],
            };
        }
    }
}