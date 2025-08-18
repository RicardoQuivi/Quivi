using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Jobs;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Application.Commands.MerchantInvoiceDocuments
{
    public class ProcessMerchantInvoiceDocumentAsyncCommand : ICommand<Task>
    {
        public int MerchantInvoiceDocumentId { get; init; }
    }

    public class ProcessMerchantInvoiceDocumentAsyncCommandHandler : ICommandHandler<ProcessMerchantInvoiceDocumentAsyncCommand, Task>
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IBackgroundJobHandler backgroundJobHandler;
        private readonly IEmailEngine emailEngine;
        private readonly IEmailService emailService;
        private readonly IIdConverter idConverter;

        public ProcessMerchantInvoiceDocumentAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                                    ICommandProcessor commandProcessor,
                                                                    IBackgroundJobHandler backgroundJobHandler,
                                                                    IEmailEngine emailEngine,
                                                                    IEmailService emailService,
                                                                    IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.backgroundJobHandler = backgroundJobHandler;
            this.emailEngine = emailEngine;
            this.emailService = emailService;
            this.idConverter = idConverter;
        }

        public async Task Handle(ProcessMerchantInvoiceDocumentAsyncCommand command)
        {
            var documentsQuery = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                Ids = [command.MerchantInvoiceDocumentId],
                Formats = [DocumentFormat.EscPos, DocumentFormat.Pdf],
                Types = [InvoiceDocumentType.OrderInvoice, InvoiceDocumentType.SurchargeInvoice, InvoiceDocumentType.CreditNote, InvoiceDocumentType.InvoiceCancellation],
                HasDownloadPath = true,
                HasPosCharge = true,
                IncludePosCharge = true,
                IncludePosChargeMerchant = true,
                PageIndex = 0,
                PageSize = 1,
            });

            var document = documentsQuery.SingleOrDefault();
            if (document == null)
                return;

            if (document.Format == DocumentFormat.EscPos)
            {
                if (new[] { InvoiceDocumentType.OrderInvoice, InvoiceDocumentType.CreditNote, InvoiceDocumentType.InvoiceCancellation }.Contains(document.DocumentType))
                    this.backgroundJobHandler.Enqueue(() => PrintDocument(document.MerchantId, NotificationMessageType.NewConsumerInvoice, document.Path!));
                return;
            }

            if (document.Format == DocumentFormat.Pdf)
            {
                var email = document.Charge?.PosCharge?.Email;
                if (string.IsNullOrWhiteSpace(email))
                    return;

                string transactionId = idConverter.ToPublicId(document.ChargeId!.Value);
                switch (document.DocumentType)
                {
                    case InvoiceDocumentType.OrderInvoice:
                        this.backgroundJobHandler.Enqueue(() => SendInvoiceEmail(email, document.Path!, new OrderInvoiceParameters
                        {
                            InvoiceName = document.DocumentId ?? transactionId,
                            Date = document.Charge!.PosCharge!.CaptureDate!.Value.ToTimeZone(document.Charge!.PosCharge!.Merchant!.TimeZone),
                            Amount = document.Charge!.PosCharge!.Payment + document.Charge!.PosCharge!.Tip + document.Charge!.PosCharge!.SurchargeFeeAmount,
                            TransactionId = transactionId,
                            MerchantName = document.Charge!.PosCharge!.Merchant!.Name,
                        }));
                        break;
                    case InvoiceDocumentType.SurchargeInvoice:
                        this.backgroundJobHandler.Enqueue(() => SendSurchargeInvoice(email, document.Path!, new SurchargeInvoiceParameters
                        {
                            InvoiceName = document.DocumentId ?? transactionId,
                            Date = document.Charge!.PosCharge!.CaptureDate!.Value.ToTimeZone(document.Charge!.PosCharge!.Merchant!.TimeZone),
                            Amount = document.Charge!.PosCharge!.Payment + document.Charge!.PosCharge!.Tip + document.Charge!.PosCharge!.SurchargeFeeAmount,
                            TransactionId = transactionId,
                            MerchantName = document.Charge!.PosCharge!.Merchant!.Name,
                        }));
                        break;
                    case InvoiceDocumentType.CreditNote:
                        break;
                    case InvoiceDocumentType.InvoiceCancellation:
                        break;
                    default:
                        break;
                }
            }
        }

        public Task PrintDocument(int merchantId, NotificationMessageType type, string url) => commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
        {
            MessageType = type,
            GetContent = async () =>
            {
                using HttpClient client = new HttpClient();
                return await client.GetStringAsync(url);
            },
            Criteria = new GetPrinterNotificationsContactsCriteria
            {
                MerchantIds = [merchantId],
                MessageTypes = [type],
                IsDeleted = false,

                PageIndex = 0,
                PageSize = null,
            },
        });

        public async Task SendInvoiceEmail(string email, string url, OrderInvoiceParameters parameters)
        {
            using HttpClient client = new HttpClient();
            var document = await client.GetByteArrayAsync(url);

            await emailService.SendAsync(new MailMessage
            {
                ToAddress = email,
                Subject = $"Fatura de compra no estabelecimento {parameters.MerchantName}",
                Body = emailEngine.OrderInvoice(parameters),
            }, [
                new MailAttachment
                {
                    Bytes = document,
                    Name = $"{parameters.InvoiceName}.pdf",
                },
            ]);
        }

        public async Task SendSurchargeInvoice(string email, string url, SurchargeInvoiceParameters parameters)
        {
            using HttpClient client = new HttpClient();
            var document = await client.GetByteArrayAsync(url);

            await emailService.SendAsync(new MailMessage
            {
                ToAddress = email,
                Subject = $"Taxa de serviço de compra no estabelecimento {parameters.MerchantName}",
                Body = emailEngine.SurchargeInvoice(parameters),
            }, [
                new MailAttachment
                {
                    Bytes = document,
                    Name = $"{parameters.InvoiceName}.pdf",
                },
            ]);
        }
    }
}