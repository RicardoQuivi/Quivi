using HashidsNet;
using Quivi.Infrastructure.Abstractions.Converters;

namespace Quivi.Infrastructure.Converters
{
    public class Hasher : IHasher
    {
        private readonly IHashids hashids;

        public Hasher()
        {
            this.hashids = new Hashids();
        }

        public string Encode(params int[] values) => this.hashids.Encode(values);
        public int[] Decode(string value) => this.hashids.Decode(value);
    }
}