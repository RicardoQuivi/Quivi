namespace Quivi.Infrastructure.Abstractions.Pos.Exceptions
{
    public class PosUnavailableException : PosException
    {
        public PosUnavailableException(Exception exception) : base($"Communication With PoS is unavailable at the moment: {exception.Message}", exception)
        {
        }
    }
}