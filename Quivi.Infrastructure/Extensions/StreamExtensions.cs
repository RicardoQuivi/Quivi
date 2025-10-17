namespace Quivi.Infrastructure.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream input)
        {
            if (input is MemoryStream ms)
                return ms.ToArray();

            using (MemoryStream ms1 = new MemoryStream())
            {
                input.CopyTo(ms1);
                return ms1.ToArray();
            }
        }
    }
}