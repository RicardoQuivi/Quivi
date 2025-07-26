using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;

namespace Quivi.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string CombinePath(this string s1, IEnumerable<string> s2)
        {
            string result = (s2 ?? Enumerable.Empty<string>()).Aggregate(s1, (seed, s) => Path.Combine(seed, s));
            return result;
        }

        public static string CombinePath(this string s1, params string[] s2) => CombinePath(s1, s2.AsEnumerable());

        public static string CombineUrl(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2;
            if (string.IsNullOrEmpty(s2))
                return s1;
            return $"{s1.TrimEnd('/')}/{s2.TrimStart('/').TrimEnd()}";
        }

        public static string CombineUrl(this string s1, IEnumerable<string> s2)
        {
            string result = (s2 ?? Enumerable.Empty<string>()).Aggregate(s1, (seed, s) => CombineUrl(seed, s));
            return result;
        }

        public static string CombineUrl(this string s1, params string[] s2) => CombineUrl(s1, s2.AsEnumerable());

        public static string AddQueryParameter(this string s, params KeyValuePair<string, string>[] parameters) => AddQueryParameter(s, parameters.AsEnumerable());

        public static string AddQueryParameter(this string s, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var aux = s.Split('?');
            if (aux.Length > 2)
                throw new Exception();

            var joinedParameters = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            if (aux.Length == 1)
                return $"{s}?{joinedParameters}";
            return $"{s}&{joinedParameters}";
        }

        public static string CleanDiacritics(this string val)
        {
            return System.Web.HttpUtility.UrlDecode(
                System.Web.HttpUtility.UrlEncode(
                    val, Encoding.GetEncoding("iso-8859-7")));
        }

        public static string ReplaceUsingRegex(this string val, string regex, string replacement)
        {
            return Regex.Replace(val, regex, replacement);
        }

        public static string Take(this IEnumerable<char> val, int charsToTake)
        {
            return Enumerable.Take(val, charsToTake).Aggregate(new StringBuilder(), (seed, c) => seed.Append(c)).ToString();
        }

        public static string CleanBankDescription(this string val)
        {
            return val
                .CleanDiacritics()
                .ReplaceUsingRegex(@"[^0-9a-zA-Z .!*-=_]+", "")
                .Take(25);
        }

        public static IEnumerable<string> SplitIntoChunks(this string str, int desiredLength)
        {
            if (str.IsNullOrEmpty() || desiredLength < 1)
            {
                throw new ArgumentException();
            }

            var maxIndex = str.Length / desiredLength;
            if (str.Length % desiredLength > 0)
                maxIndex++;

            return Enumerable.Range(0, maxIndex)
                            .Select(i => string.Join("", str.Skip(i * desiredLength).Take(desiredLength)));
        }

        public static string CapitalizeFirstLetter(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToUpper();
            return s.Remove(1).ToUpper() + s.Substring(1);
        }

        public static string Ellipsis(this string str, int maxChars)
        {
            return str.Length <= maxChars ? str : str.Substring(0, maxChars) + "..";
        }
    }
}
