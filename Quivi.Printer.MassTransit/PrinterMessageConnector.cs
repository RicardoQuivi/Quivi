using MassTransit;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Printer.Contracts;

namespace Quivi.Printer.MassTransit
{
    public class PrinterMessageConnector : IPrinterMessageConnector, IConsumer<PrintMessageStatusUpdate>
    {
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly IIdConverter idConverter;
        private readonly IPrintingStatusUpdater printerStatusUpdater;

        public PrinterMessageConnector(ISendEndpointProvider sendEndpointProvider, IIdConverter idConverter, IPrintingStatusUpdater printerStatusUpdater)
        {
            this.sendEndpointProvider = sendEndpointProvider;
            this.idConverter = idConverter;
            this.printerStatusUpdater = printerStatusUpdater;
        }

        public async Task SendMessage(string workerIdentifier, Message message)
        {
            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:printers.{workerIdentifier}"));
            await endpoint.Send(new PrintMessage
            {
                Id = idConverter.ToPublicId(message.PrinterNotificationMessageId),
                MerchantId = idConverter.ToPublicId(message.MerchantId),
                Content = message.Content,
                Targets = message.Targets.Select(t => new PrintMessageTarget
                {
                    Id = idConverter.ToPublicId(t.PrinterNotificationsContactId),
                    Address = t.Address,
                }).ToList(),
                CreatedDate = message.CreatedDate,
            });
        }

        public async Task Consume(ConsumeContext<PrintMessageStatusUpdate> context)
        {
            var submerchantId = idConverter.FromPublicId(context.Message.MerchantId);
            var messageId = idConverter.FromPublicId(context.Message.MessageId);
            var targetId = idConverter.FromPublicId(context.Message.PrinterId);

            await printerStatusUpdater.ProcessStatus(submerchantId, messageId, targetId, context.Message.UtcDate, context.Message.Status);
        }
    }
}