using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Extensions.Pos
{
    public class SessionItemComparer : IEqualityComparer<SessionItem>, IEqualityComparer<BaseSessionItem>, IEqualityComparer<SessionExtraItem>
    {
        public static List<SessionItem> Compress(IEnumerable<SessionItem> items)
        {
            var compressor = new SessionItemComparer();
            return items.GroupBy(g => g, compressor).Select(t =>
            {
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

        private List<SessionExtraItem> Compress(IEnumerable<SessionExtraItem> items)
        {
            return items.GroupBy(g => g, this).Select(t => new SessionExtraItem
            {
                ModifierGroupId = t.Key.ModifierGroupId,
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

            var extraComparer = (this as IEqualityComparer<SessionExtraItem>);
            foreach (var (extraA, extraB) in groupA.Zip(groupB, (extraA, extraB) => (extraA, extraB)))
                if (extraA.Quantity != extraB.Quantity != comparer.Equals(extraA, extraA) == false)
                    return false;
            return true;
        }

        int IEqualityComparer<SessionItem>.GetHashCode(SessionItem obj)
        {
            var comparer = (this as IEqualityComparer<BaseSessionItem>);
            var priceAfterDiscount = GetPriceAfterDiscount(obj);
            var result = comparer.GetHashCode(obj) ^ priceAfterDiscount.GetHashCode();

            var extracomparer = (this as IEqualityComparer<SessionExtraItem>);
            foreach (var extra in Compress(obj.Extras))
                result ^= comparer.GetHashCode(extra);
            return result;
        }

        bool IEqualityComparer<SessionExtraItem>.Equals(SessionExtraItem? a, SessionExtraItem? b)
        {
            var comparer = (this as IEqualityComparer<BaseSessionItem>);

            if (comparer.Equals(a, b) == false)
                return false;

            if (a == null || b == null)
                return a == b;

            if (a.ModifierGroupId != b.ModifierGroupId)
                return false;

            return true;
        }

        int IEqualityComparer<SessionExtraItem>.GetHashCode(SessionExtraItem obj)
        {
            var comparer = (this as IEqualityComparer<BaseSessionItem>);
            var result = comparer.GetHashCode(obj) ^ obj.ModifierGroupId.GetHashCode();
            return result;
        }

        bool IEqualityComparer<BaseSessionItem>.Equals(BaseSessionItem? x, BaseSessionItem? y) => x?.MenuItemId == y?.MenuItemId && x?.Price == y?.Price;
        int IEqualityComparer<BaseSessionItem>.GetHashCode(BaseSessionItem obj) => obj.MenuItemId.GetHashCode() ^ obj.Price.GetHashCode();
    }

    internal class ConvertedTuple<TSession, T> where TSession : BaseSessionItem
    {
        public required TSession SessionItem { get; init; }
        public required T Item { get; init; }
    }

    public record SessionExtraItem<T> : SessionExtraItem
    {
        public required IEnumerable<T> Source { get; init; }
    }

    public record SessionItem<T, TExtra> : BaseSessionItem
    {
        public decimal Discount { get; init; }
        public required IEnumerable<SessionExtraItem<TExtra>> Extras { get; init; }
        public required IEnumerable<T> Source { get; init; }
    }

    public class SessionItemComparer<T> : SessionItemComparer, IEqualityComparer<ConvertedTuple<SessionItem, T>>
    {
        private class ExtraSessionItemComparer<TChild> : SessionItemComparer, IEqualityComparer<ConvertedTuple<SessionExtraItem, TChild>>
        {
            public static IEnumerable<SessionExtraItem<TChild>> Compress(IEnumerable<TChild> items, Func<TChild, SessionExtraItem> converter, Func<TChild, int> getModifierGroupId)
            {
                var compressor = new ExtraSessionItemComparer<TChild>();
                return items.Select(s => new ConvertedTuple<SessionExtraItem, TChild>
                {
                    SessionItem = converter(s),
                    Item = s,
                }).GroupBy(g => g, compressor).Select(t =>
                {
                    var first = t.First();
                    var firstSessionItem = first.SessionItem;
                    var firstItem = first.Item;

                    return new SessionExtraItem<TChild>
                    {
                        ModifierGroupId = getModifierGroupId(firstItem),
                        MenuItemId = t.Key.SessionItem.MenuItemId,
                        Price = t.Key.SessionItem.Price,
                        Quantity = t.Sum(t2 => t2.SessionItem.Quantity),
                        Source = t.Select(s => s.Item),
                    };
                }).Where(t => t.Quantity != 0);
            }

            bool IEqualityComparer<ConvertedTuple<SessionExtraItem, TChild>>.Equals(ConvertedTuple<SessionExtraItem, TChild>? x, ConvertedTuple<SessionExtraItem, TChild>? y) => (this as IEqualityComparer<SessionExtraItem>).Equals(x?.SessionItem, y?.SessionItem);
            int IEqualityComparer<ConvertedTuple<SessionExtraItem, TChild>>.GetHashCode(ConvertedTuple<SessionExtraItem, TChild> obj) => (this as IEqualityComparer<SessionExtraItem>).GetHashCode(obj.SessionItem);
        }

        public static IEnumerable<SessionItem<T, TChild>> Compress<TChild>(IEnumerable<T> items,
                                                                                Func<T, SessionItem> parentConverter,
                                                                                Func<T, IEnumerable<TChild>> getModifiers,
                                                                                Func<TChild, SessionExtraItem> extraConverter,
                                                                                Func<TChild, int> getModifierGroupId)
        {
            var compressor = new SessionItemComparer<T>();
            return items.Select(s => new ConvertedTuple<SessionItem, T>
            {
                SessionItem = parentConverter(s),
                Item = s,
            }).GroupBy(g => g, compressor).Select(t =>
            {
                var first = t.First();
                var firstSessionItem = first.SessionItem;
                var firstItem = first.Item;

                return new SessionItem<T, TChild>
                {
                    Discount = firstSessionItem.Discount,
                    MenuItemId = t.Key.SessionItem.MenuItemId,
                    Price = t.Key.SessionItem.Price,
                    Quantity = t.Sum(t2 => t2.SessionItem.Quantity),
                    Extras = ExtraSessionItemComparer<TChild>.Compress(getModifiers(firstItem), extraConverter, getModifierGroupId).Select(t => new SessionExtraItem<TChild>
                    {
                        MenuItemId = t.MenuItemId,
                        ModifierGroupId = t.ModifierGroupId,
                        Price = t.Price,
                        Quantity = t.Quantity,
                        Source = t.Source,
                    }),
                    Source = t.Select(s => s.Item),
                };
            }).Where(t => t.Quantity != 0);
        }

        bool IEqualityComparer<ConvertedTuple<SessionItem, T>>.Equals(ConvertedTuple<SessionItem, T>? x, ConvertedTuple<SessionItem, T>? y) => (this as IEqualityComparer<SessionItem>).Equals(x?.SessionItem, y?.SessionItem);
        int IEqualityComparer<ConvertedTuple<SessionItem, T>>.GetHashCode(ConvertedTuple<SessionItem, T> obj) => (this as IEqualityComparer<SessionItem>).GetHashCode(obj.SessionItem);
    }

    public static class SessionItemExtensions
    {
        public static IEnumerable<SessionItem> Compress(this IEnumerable<SessionItem> items) => SessionItemComparer.Compress(items);

        public static SessionItem AsSessionItem(this OrderMenuItem s) => new SessionItem
        {
            MenuItemId = s.MenuItemId,
            Price = s.FinalPrice,
            Quantity = s.Quantity,
            Discount = PriceHelper.CalculateDiscountPercentage(s.OriginalPrice, s.FinalPrice),
            Extras = s.Modifiers?.Select(e => new SessionExtraItem
            {
                ModifierGroupId = e.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.MenuItemModifierGroup)}"),
                MenuItemId = e.MenuItemId,
                Price = e.FinalPrice,
                Quantity = s.Quantity == 0 ? 0 : e.Quantity / s.Quantity,
            }) ?? [],
        };

        public static IEnumerable<SessionItem> AsSessionItems(this IEnumerable<OrderMenuItem> items) => SessionItemComparer.Compress(items.Where(i => i.ParentOrderMenuItemId.HasValue == false).Select(AsSessionItem));

        public static IEnumerable<SessionItem> AsSessionItems(this IEnumerable<PosChargeInvoiceItem> items)
        {
            return SessionItemComparer.Compress(items.Select(s => new SessionItem
            {
                MenuItemId = s.OrderMenuItem!.MenuItemId,
                Price = s.OrderMenuItem.FinalPrice,
                Quantity = s.Quantity,
                Discount = PriceHelper.CalculateDiscountPercentage(s.OrderMenuItem.OriginalPrice, s.OrderMenuItem.FinalPrice),
                Extras = s.ChildrenPosChargeInvoiceItems?.Select(e => new SessionExtraItem
                {
                    ModifierGroupId = e.OrderMenuItem!.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.OrderMenuItem.MenuItemModifierGroup)}"),
                    MenuItemId = e.OrderMenuItem!.Id,
                    Price = e.OrderMenuItem!.FinalPrice,
                    Quantity = e.Quantity,
                }) ?? [],
            }));
        }

        public static IEnumerable<SessionItem<OrderMenuItem, OrderMenuItem>> AsConvertedSessionItems(this IEnumerable<OrderMenuItem> items)
        {
            var result = SessionItemComparer<OrderMenuItem>.Compress(items.Where(i => i.ParentOrderMenuItemId.HasValue == false), s => new SessionItem
            {
                MenuItemId = s.MenuItemId,
                Price = s.FinalPrice,
                Quantity = s.Quantity,
                Discount = PriceHelper.CalculateDiscountPercentage(s.OriginalPrice, s.FinalPrice),
                Extras = s.Modifiers?.Select(e => new SessionExtraItem
                {
                    ModifierGroupId = e.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.MenuItemModifierGroup)}"),
                    MenuItemId = e.MenuItemId,
                    Price = e.FinalPrice,
                    Quantity = e.ParentOrderMenuItem!.Quantity == 0 ? 0 : e.Quantity / e.ParentOrderMenuItem!.Quantity,
                }) ?? [],
            }, s => s.Modifiers ?? [], e => new SessionExtraItem
            {
                ModifierGroupId = e.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.MenuItemModifierGroup)}"),
                MenuItemId = e.MenuItemId,
                Price = e.FinalPrice,
                Quantity = e.ParentOrderMenuItem!.Quantity == 0 ? 0 : e.Quantity / e.ParentOrderMenuItem!.Quantity,
            }, e => e.MenuItemModifierGroupId ?? throw new Exception($"The extra needs to belong to a {nameof(e.MenuItemModifierGroup)}"));
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

        public static decimal GetUnitPrice<T, TExtra>(this SessionItem<T, TExtra> item) => item.Price + item.Extras.Sum(child => child.Price * child.Quantity);
        public static decimal GetUnitPrice<T, TExtra>(this SessionItem<T, TExtra> item, int maxDecimalPlaces) => Math.Round(item.GetUnitPrice(), maxDecimalPlaces);
    }
}