namespace Quivi.Application.Services.Exceptions
{
    public class NoBalanceException : Exception
    {
        public decimal CurrentBalance { get; init; }
        public decimal RequiredBalance { get; init; }
    }
}