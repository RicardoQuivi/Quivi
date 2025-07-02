using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PrinterMessageMapperHandler : IMapperHandler<PrinterMessageTarget, Dtos.PrinterMessage>
    {
        private readonly IIdConverter idConverter;

        public PrinterMessageMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public PrinterMessage Map(PrinterMessageTarget model)
        {
            return new PrinterMessage
            {
                MessageId = idConverter.ToPublicId(model.PrinterNotificationMessageId),
                PrinterId = idConverter.ToPublicId(model.PrinterNotificationsContactId),
                Type = model.PrinterNotificationMessage!.MessageType,
                Statuses = MapStatuses(model)
            };
        }

        private IReadOnlyDictionary<PrinterMessageStatus, DateTimeOffset> MapStatuses(PrinterMessageTarget model)
        {
            var result = new Dictionary<PrinterMessageStatus, DateTimeOffset>
            {
                { PrinterMessageStatus.Created, new DateTimeOffset(model.CreatedDate, TimeSpan.Zero) }
            };

            if (model.RequestedAt.HasValue)
                result.Add(PrinterMessageStatus.Processing, new DateTimeOffset(model.RequestedAt.Value, TimeSpan.Zero));

            if (model.FinishedAt.HasValue)
            {
                PrinterMessageStatus status = PrinterMessageStatus.Failed;
                switch (model.Status)
                {
                    case AuditStatus.Failed:
                        status = PrinterMessageStatus.Failed;
                        break;
                    case AuditStatus.TimedOut:
                        status = PrinterMessageStatus.TimedOut;
                        break;
                    case AuditStatus.Unreachable:
                        status = PrinterMessageStatus.Unreachable;
                        break;
                    case AuditStatus.Success:
                        status = PrinterMessageStatus.Success;
                        break;
                    case AuditStatus.Pending: throw new Exception("This should never happen, the status should be set to success or failed.");
                }
                result.Add(status, new DateTimeOffset(model.FinishedAt.Value, TimeSpan.Zero));
            }

            return result;
        }
    }
}
