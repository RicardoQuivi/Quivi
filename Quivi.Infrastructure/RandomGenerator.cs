using Quivi.Infrastructure.Abstractions;

namespace Quivi.Infrastructure
{
    public class RandomGenerator : IRandomGenerator
    {
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

        public Guid Guid() => System.Guid.NewGuid();

        public string String(int length)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(Chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
