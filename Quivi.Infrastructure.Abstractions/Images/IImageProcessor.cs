namespace Quivi.Infrastructure.Abstractions.Images
{
    public interface IImageProcessor
    {
        Stream Compress(Stream stream, int minSize);
    }
}
