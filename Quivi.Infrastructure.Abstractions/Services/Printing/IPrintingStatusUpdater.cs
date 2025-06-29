using Quivi.Printer.Contracts;

namespace Quivi.Infrastructure.Abstractions.Services.Printing
{
    public interface IPrintingStatusUpdater
    {
        Task ProcessStatus(int merchantId, int printerNotificationMessageId, int printerNotificationsContactId, DateTime utcDate, PrintStatus status);
    }
}