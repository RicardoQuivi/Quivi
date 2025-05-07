namespace Quivi.Infrastructure.Exceptions
{
    public class InvalidPublicIdException : Exception
    {
        public InvalidPublicIdException(string publicId) : base($"The Public Id {publicId} is not valid")
        {
        }
    }
}
