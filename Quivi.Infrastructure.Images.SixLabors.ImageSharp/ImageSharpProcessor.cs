using Quivi.Infrastructure.Abstractions.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Quivi.Infrastructure.Images.SixLabors.ImageSharp
{
    public class ImageSharpProcessor : IImageProcessor
    {
        public Stream Compress(Stream stream, int minSize)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            using var image = Image.Load(stream);
            var size = new[] { minSize, image.Width, image.Height }.Min();
            image.Mutate(x => x.Resize(size, size));

            if (image.Metadata.DecodedImageFormat == null)
                throw new Exception();

            var outputStream = new MemoryStream();
            image.Save(outputStream, image.Metadata.DecodedImageFormat);
            outputStream.Position = 0;

            return outputStream;
        }
    }
}
