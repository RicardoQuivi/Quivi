namespace Quivi.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrIfEmpty<T>(this IEnumerable<T>? items, IEnumerable<T> fallback)
        {
            if (items?.Any() != true)
                return fallback;
            return items;
        }

        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? items) => items ?? [];
    }
}