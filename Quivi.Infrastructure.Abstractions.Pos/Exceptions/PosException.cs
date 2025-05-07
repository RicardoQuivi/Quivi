namespace Quivi.Infrastructure.Abstractions.Pos.Exceptions
{
    public class PosException : Exception
    {
        public PosException() : base()
        {
        }

        public PosException(string message) : base(message)
        {
        }

        public PosException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
