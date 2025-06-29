using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

namespace Quivi.Infrastructure.Pos.ESCPOS_NET
{
    public static class EscPosExtentions
    {
        public static byte[][] ConcatIf(this byte[][] left, bool condition, Func<byte[][]> right)
        {
            return condition
                ? left.Concat(right()).ToArray()
                : left;
        }

        public static byte[][] ConcatWith(this byte[][] left, params byte[][] right)
        {
            return left.Concat(right).ToArray();
        }

        public static byte[][] ConcatWithForeach<T>(this byte[][] left, IEnumerable<T> list, Func<T, byte[][]> listAction)
        {
            return left.Concat(list.SelectMany(listAction).ToArray()).ToArray();
        }

        public static byte[][] StartConcat(this BaseCommandEmitter emitter, params byte[][] value) => value;

        public static string Encode(this byte[][] commands) => Convert.ToBase64String(ByteSplicer.Combine(commands));

        public static string PadRightAndTruncate(this string value, int desiredSize, string truncateSufix = "...")
        {
            var safeValue = $"{value}";
            if (safeValue.Length <= desiredSize)
                return safeValue.PadRight(desiredSize);

            return safeValue
                .Take(Math.Max(0, desiredSize - truncateSufix.Length))
                .Concat(truncateSufix)
                .Aggregate("", (l, r) => $"{l}{r}");

        }
    }
}
