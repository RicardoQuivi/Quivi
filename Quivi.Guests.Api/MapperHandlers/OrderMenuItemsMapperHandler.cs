using Quivi.Application.Extensions.Pos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class OrderMenuItemsMapperHandler : IMapperHandler<IEnumerable<OrderMenuItem>, IEnumerable<Dtos.SessionItem>>
    {
        //TODO: When multi currency is supported, then decimal places should be whatever higher number decimal places of supported currencies
        const int decimalPlaces = 2;
        public static readonly int multiplier = (int)Math.Pow(10, decimalPlaces);


        private readonly IIdConverter idConverter;
        private readonly IHasher hasher;

        public OrderMenuItemsMapperHandler(IIdConverter idConverter,
                                    IHasher hasher)
        {
            this.idConverter = idConverter;
            this.hasher = hasher;
        }


        public IEnumerable<Dtos.SessionItem> Map(IEnumerable<OrderMenuItem> model)
        {
            var sessionItems = model.AsConvertedSessionItems();

            foreach (var item in sessionItems)
            {
                var first = item.Source.First();
                var paidQuantity = item.Source.SelectMany(s => s.PosChargeInvoiceItems!).Sum(s => s.Quantity);
                var unpaidQuantity = item.Quantity - paidQuantity;

                if (unpaidQuantity > 0)
                    yield return GetItem(multiplier, item, first, unpaidQuantity, false);

                if (paidQuantity > 0)
                    yield return GetItem(multiplier, item, first, paidQuantity, true);
            }
        }

        private Dtos.SessionItem GetItem(int multiplier, SessionItem<OrderMenuItem> model, OrderMenuItem first, decimal unpaidQuantity, bool isPaid)
        {
            var extras = model.Extras.Select(e =>
            {
                var extra = e.Source.First();
                int[] key = [extra.MenuItemId, (int)(extra.FinalPrice * multiplier), (int)(extra.OriginalPrice * multiplier)];
                return new
                {
                    Key = key,
                    Item = new Dtos.BaseSessionItem
                    {
                        OriginalPrice = extra.OriginalPrice,
                        MenuItemId = idConverter.ToPublicId(e.MenuItemId),
                        Price = e.Price,
                        Quantity = e.Quantity,
                    },
                };
            }).ToDictionary(e => e.Key, e => e.Item);

            IEnumerable<int> key = [model.MenuItemId, (int)(first.FinalPrice * multiplier), (int)(first.OriginalPrice * multiplier)];
            key = key.Concat(extras.SelectMany(e => e.Key));
            return new Dtos.SessionItem
            {
                Id = hasher.Encode(key.ToArray()),
                IsPaid = isPaid,
                OriginalPrice = first.OriginalPrice,

                MenuItemId = idConverter.ToPublicId(model.MenuItemId),
                Price = model.Price,
                Quantity = unpaidQuantity,
                DiscountPercentage = model.Discount,
                Extras = extras.Values,

                LastModified = new DateTimeOffset(model.Source.Select(e => e.ModifiedDate).Max(), TimeSpan.Zero),
            };
        }
    }
}