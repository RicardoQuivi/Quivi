namespace Quivi.Infrastructure.Abstractions.Services.Printing
{
    public interface IPrinterMessageConnector
    {
        Task SendMessage(string workerIdentifier, Message message);
    }
}