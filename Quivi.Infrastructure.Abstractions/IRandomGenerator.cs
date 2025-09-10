namespace Quivi.Infrastructure.Abstractions
{
    public interface IRandomGenerator
    {
        Guid Guid();
        string String(int length);
        string Password(int minLenght);
    }
}
