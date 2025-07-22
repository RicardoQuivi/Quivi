using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class PrinterMessageTargetMapperHandler : IMapperHandler<PrinterMessageTarget, Dtos.Printer>
    {
        private readonly IIdConverter idConverter;

        public PrinterMessageTargetMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }
        public Dtos.Printer Map(PrinterMessageTarget model)
        {
            return new Dtos.Printer
            {
                Id = idConverter.ToPublicId(model.PrinterNotificationsContact!.NotificationsContactId),
                Name = model.PrinterNotificationsContact!.Name,
                PrintConsumerInvoice = model.PrinterNotificationsContact.BaseNotificationsContact!.SubscribedNotifications.HasFlag(NotificationMessageType.NewConsumerInvoice),
                PrintConsumerBill = model.PrinterNotificationsContact.BaseNotificationsContact!.SubscribedNotifications.HasFlag(NotificationMessageType.ConsumerBill),
                PrintPreparationRequest = model.PrinterNotificationsContact.BaseNotificationsContact!.SubscribedNotifications.HasFlag(NotificationMessageType.NewPreparationRequest),
                CanOpenCashDrawer = model.PrinterNotificationsContact.BaseNotificationsContact!.SubscribedNotifications.HasFlag(NotificationMessageType.OpenCashDrawer),
                CanPrintCloseDayTotals = model.PrinterNotificationsContact.BaseNotificationsContact!.SubscribedNotifications.HasFlag(NotificationMessageType.EndOfDayClosing),
            };
        }
    }
}