using Quivi.Application.Pos.Items;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Extensions.Pos
{
    public class SessionItemComparer : IEqualityComparer<SessionItem>, IEqualityComparer<BaseSessionItem>
    {
        public static List<SessionItem> Compress(IEnumerable<SessionItem> items)
        {
            var compressor = new SessionItemComparer();
            return items.GroupBy(g => g, compressor).Select(t => {
                var first = t.First();
                return new SessionItem
                {
                    Discount = first.Discount,
                    MenuItemId = t.Key.MenuItemId,
                    Price = t.Key.Price,
                    Quantity = t.Sum(t2 => t2.Quantity),
                    Extras = compressor.Compress(first.Extras),
                };
            }).Where(t => t.Quantity != 0).ToList();
        }

        private List<BaseSessionItem> Compress(IEnumerable<BaseSessionItem> items)
        {
            return items.GroupBy(g => g, this).Select(t => new BaseSessionItem
            {
                MenuItemId = t.Key.MenuItemId,
                Price = t.Key.Price,
                Quantity = t.Sum(t2 => t2.Quantity),
            }).Where(t => t.Quantity != 0).ToList();
        }

        private decimal GetPriceAfterDiscount(SessionItem item) => item.Price * (100.0m - item.Discount) / 100.0m;

        bool IEqualityComparer<SessionItem>.Equals(SessionItem? a, SessionItem? b)
        {
            var comparer = (this as IEqualityComparer<BaseSessionItem>);

            if (comparer.Equals(a, b) == false)
                return false;

            if (a == null || b == null)
                return a == b;

            if (GetPriceAfterDiscount(a) != GetPriceAfterDiscount(b))
                return false;

            var groupA = Compress(a.Extras);
            var groupB = Compress(b.Extras);

            if (groupA.Count != groupB.Count)
                return false;

            foreach (var (extraA, extraB) in groupA.Zip(groupB, (extraA, extraB) => (extraA, extraB)))
                if (extraA.Quantity != extraB.Quantity != comparer.Equals(extraA, extraA) == false)
                    return false;
            return true;
        }

        bool IEqualityComparer<BaseSessionItem>.Equals(BaseSessionItem? x, BaseSessionItem? y) => x?.MenuItemId == y?.MenuItemId && x?.Price == y?.Price;

        int IEqualityComparer<SessionItem>.GetHashCode(SessionItem obj)
        {
            var comparer = (this as IEqualityComparer<BaseSessionItem>);
            var priceAfterDiscount = GetPriceAfterDiscount(obj);
            var result = comparer.GetHashCode(obj) ^ priceAfterDiscount.GetHashCode();
            foreach (var extra in Compress(obj.Extras))
                result ^= comparer.GetHashCode(extra);
            return result;
        }

        int IEqualityComparer<BaseSessionItem>.GetHashCode(BaseSessionItem obj) => obj.MenuItemId.GetHashCode() ^ obj.Price.GetHashCode();
    }

    internal class ConvertedTuple<T>
    {
        public required SessionItem SessionItem { get; init; }
        public required T Item { get; init; }
    }

    public record BaseSessionItem<T> : BaseSessionItem
    {
        public required IEnumerable<T> Source { get; init; }
    }

    public record SessionItem<T> : BaseSessionItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<BaseSessionItem<T>> Extras { get; init; }
        public required IEnumerable<T> Source { get; init; }
    }

    public class SessionItemComparer<T> : SessionItemComparer, IEqualityComparer<ConvertedTuple<T>>
    {
        public static IEnumerable<SessionItem<T>> Compress(IEnumerable<T> items, Func<T, SessionItem> converter, Func<T, IEnumerable<T>> getModifiers)
        {
            var compressor = new SessionItemComparer<T>();
            return items.Select(s => new ConvertedTuple<T>
            {
                SessionItem = converter(s),
                Item = s,
            }).GroupBy(g => g, compressor).Select(t => {
                var first = t.First();
                var firstSessionItem = first.SessionItem;
                var firstItem = first.Item;

                return new SessionItem<T>
                {
                    Discount = firstSessionItem.Discount,
                    MenuItemId = t.Key.SessionItem.MenuItemId,
                    Price = t.Key.SessionItem.Price,
                    Quantity = t.Sum(t2 => t2.SessionItem.Quantity),
                    Extras = SessionItemComparer<T>.Compress(getModifiers(firstItem), converter, getModifiers).Select(s => new BaseSessionItem<T>
                    {
                        MenuItemId = s.MenuItemId,
                        Price = s.Price,
                        Quantity = s.Quantity,
                        Source = s.Source,
                    }),

                    Source = t.Select(s => s.Item),
                };
            }).Where(t => t.Quantity != 0);
        }

        bool IEqualityComparer<ConvertedTuple<T>>.Equals(ConvertedTuple<T>? x, ConvertedTuple<T>? y) => (this as IEqualityComparer<SessionItem>).Equals(x?.SessionItem, y?.SessionItem);

        int IEqualityComparer<ConvertedTuple<T>>.GetHashCode(ConvertedTuple<T> obj) => (this as IEqualityComparer<SessionItem>).GetHashCode(obj.SessionItem);
    }

    public static class SessionItemExtensions
    {
        public static IEnumerable<SessionItem> Compress(this IEnumerable<SessionItem> items)
        {
            var result = SessionItemComparer.Compress(items);
            return result;
        }

        public static SessionItem AsSessionItem(this OrderMenuItem s)
        {
            return new SessionItem
            {
                MenuItemId = s.MenuItemId,
                Price = s.FinalPrice,
                Quantity = s.Quantity,
                Discount = PriceHelper.CalculateDiscountPercentage(s.OriginalPrice, s.FinalPrice),
                Extras = s.Modifiers?.Select(e => new BaseSessionItem
                {
                    MenuItemId = e.MenuItemId,
                    Price = e.FinalPrice,
                    Quantity = e.Quantity,
                }) ?? [],
            };
        }

        public static IEnumerable<SessionItem> AsSessionItems(this IEnumerable<OrderMenuItem> items)
        {
            var result = SessionItemComparer.Compress(items.Select(AsSessionItem));
            return result;
        }

        public static IEnumerable<SessionItem> AsSessionItems(this IEnumerable<PosChargeInvoiceItem> items)
        {
            var result = SessionItemComparer.Compress(items.Select(s => new SessionItem
            {
                MenuItemId = s.OrderMenuItem!.MenuItemId,
                Price = s.OrderMenuItem.FinalPrice,
                Quantity = s.Quantity,
                Discount = PriceHelper.CalculateDiscountPercentage(s.OrderMenuItem.OriginalPrice, s.OrderMenuItem.FinalPrice),
                Extras = s.ChildrenPosChargeInvoiceItems?.Select(e => new BaseSessionItem
                {
                    MenuItemId = e.OrderMenuItem!.MenuItemId,
                    Price = e.OrderMenuItem!.FinalPrice,
                    Quantity = e.Quantity,
                }) ?? [],
            }));
            return result;
        }

        public static IEnumerable<SessionItem<OrderMenuItem>> AsConvertedSessionItems(this IEnumerable<OrderMenuItem> items)
        {
            var result = SessionItemComparer<OrderMenuItem>.Compress(items, s => new SessionItem
            {
                MenuItemId = s.MenuItemId,
                Price = s.FinalPrice,
                Quantity = s.Quantity,
                Discount = PriceHelper.CalculateDiscountPercentage(s.OriginalPrice, s.FinalPrice),
                Extras = s.Modifiers?.Select(e => new BaseSessionItem
                {
                    MenuItemId = e.MenuItemId,
                    Price = e.FinalPrice,
                    Quantity = e.Quantity,
                }) ?? [],
            }, s => s.Modifiers ?? []);
            return result;
        }

        public static IEnumerable<PaidSessionItem> AsPaidSessionItems(this IEnumerable<OrderMenuItem> items)
        {
            var sessionItems = items.AsConvertedSessionItems();

            foreach (var model in sessionItems)
            {
                var first = model.Source.First();
                var paidQuantity = model.Source.SelectMany(s => s.PosChargeInvoiceItems ?? []).Sum(s => s.Quantity);
                var unpaidQuantity = model.Quantity - paidQuantity;

                yield return new PaidSessionItem
                {
                    MenuItemId = model.MenuItemId,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    PaidQuantity = paidQuantity,
                    Discount = model.Discount,
                    Extras = model.Extras,
                };
            }
        }

        public static decimal GetUnitPrice(this SessionItem item) => item.Price + item.Extras.Sum(child => child.Price * child.Quantity);
        public static decimal GetUnitPrice(this SessionItem item, int maxDecimalPlaces) => Math.Round(item.GetUnitPrice(), maxDecimalPlaces);
    }
}