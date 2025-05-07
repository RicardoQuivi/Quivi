using HashidsNet;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Exceptions;

namespace Quivi.Infrastructure.Converters
{
    public class IdConverter : IIdConverter
    {
        private readonly IHashids hashids;

        public IdConverter(string hashIdsSalt)
        {
            this.hashids = new Hashids(hashIdsSalt, 8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        }

        public int FromPublicId(string id)
        {
            if (TryFromPublicId(id, out int parsedId))
                return parsedId;

            throw new InvalidPublicIdException(id);
        }

        private bool TryFromPublicId(string id, out int parsedId)
        {
            parsedId = 0;

            var aux = hashids.Decode(id);
            if (aux.Any() == false || aux.Skip(1).Any())
                return false;

            parsedId = aux.Single();
            return true;
        }

        public string ToPublicId(int id) => hashids.Encode(id);
    }
}
