using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;

namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface IPrinterNotificationMessagesRepository : IRepository<PrinterNotificationMessage, GetPrinterNotificationMessagesCriteria>
    {
    }
}