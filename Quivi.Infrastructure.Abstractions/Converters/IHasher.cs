namespace Quivi.Infrastructure.Abstractions.Converters
{
    public interface IHasher
    {
        string Encode(params int[] values);
        int[] Decode(string value);
    }
}
