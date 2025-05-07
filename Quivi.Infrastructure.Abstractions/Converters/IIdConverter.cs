namespace Quivi.Infrastructure.Abstractions.Converters
{
    public interface IIdConverter
    {
        string ToPublicId(int id);
        int FromPublicId(string id);
    }
}
