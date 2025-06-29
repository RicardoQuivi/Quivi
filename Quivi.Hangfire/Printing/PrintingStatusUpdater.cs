using Quivi.Application.Commands.PrinterMessageTargets;
using Quivi.Domain.Entities.Notifications;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Printing;
using Quivi.Printer.Contracts;

namespace Quivi.Hangfire.Printing
{
    public class PrintingStatusUpdater : IPrintingStatusUpdater
    {
        private readonly ICommandProcessor commandProcessor;

        public PrintingStatusUpdater(ICommandProcessor commandProcessor) 
        {
            this.commandProcessor = commandProcessor;
        }

        public async Task ProcessStatus(int merchantId, int printerNotificationMessageId, int printerNotificationsContactId, DateTime utcDate, PrintStatus status)
        {
            await commandProcessor.Execute(new UpdatePrinterMessageTargetsAsyncCommand
            {
                Criteria = new GetPrinterMessageTargetsCriteria
                {
                    PrinterNotificationMessageIds = [printerNotificationMessageId],
                    PrinterNotificationsContactIds = [printerNotificationsContactId],
                    PageSize = 1,
                    PageIndex = 0,
                },
                UpdateAction = (e) =>
                {
                    switch (status)
                    {
                        case PrintStatus.Started:
                            if (e.RequestedAt.HasValue == false)
                                e.RequestedAt = utcDate;
                            break;

                        case PrintStatus.Success:
                        case PrintStatus.Unreachable:
                        case PrintStatus.Failed:
                            if (e.FinishedAt.HasValue == false)
                            {
                                e.FinishedAt = utcDate;
                                if(status == PrintStatus.Failed)
                                    e.Status = AuditStatus.Failed;
                                else if(status == PrintStatus.Success)
                                    e.Status = AuditStatus.Success;
                                else if(status == PrintStatus.Unreachable)
                                    e.Status = AuditStatus.Unreachable;
                            }
                            break;
                        default:
                            break;
                    }
                    if(status == PrintStatus.Success)
                    {
                        e.FinishedAt = utcDate;
                        e.Status = AuditStatus.Success;
                    }
                    return Task.CompletedTask;
                }
            });
        }
    }
}
