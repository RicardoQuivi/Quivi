using FacturaLusa.v2.Dtos;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Abstractions;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Commands;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Models;
using Quivi.Infrastructure.Pos.FacturaLusa.v2.Queries;

namespace Quivi.Infrastructure.Pos.FacturaLusa.v2
{
    public class FacturaLusaInvoiceGateway : IInvoiceGateway
    {
        public required IQueryProcessor QueryProcessor { get; init; }
        public required ICommandProcessor CommandProcessor { get; init; }
        private readonly Lazy<IFacturaLusaService> facturalusaService;

        public string GatewayCode => "FL";

        public FacturaLusaInvoiceGateway(IFacturaLusaServiceFactory facturalusaServiceFactory, string accessToken, string accountUuid)
        {
            facturalusaService = new Lazy<IFacturaLusaService>(() => facturalusaServiceFactory.Create(accessToken, accountUuid));
        }

        public Task<bool> HealthCheck() => QueryProcessor.Execute(new HealthCheckAsyncQuery
        {
            Service = facturalusaService.Value,
        });

        #region Invoice Receipt
        public async Task<InvoiceReceipt> CreateInvoiceReceipt(InvoiceReceipt invoice)
        {
            var document = await CommandProcessor.Execute(new CreateInvoiceReceiptAsyncCommand
            {
                Service = facturalusaService.Value,
                Data = Convert(invoice),
                Format = DocumentFormat.POS,
            });

            var result = ConvertToInvoiceReceipt(document);
            return result;
        }

        public async Task<InvoiceReceipt> GetInvoiceReceipt(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetSaleAsyncQuery
            {
                Service = facturalusaService.Value,
                DocumentId = documentId,
            });

            var result = ConvertToInvoiceReceipt(document!);
            return result;
        }

        public Task<byte[]> GetInvoiceReceiptFile(string documentId, DocumentFileFormat format) => GetFileContent(documentId, format);
        #endregion

        #region Consumer Bill
        public async Task<ConsumerBill> CreateConsumerBillReceipt(ConsumerBill consumerBill)
        {
            var document = await CommandProcessor.Execute(new CreateConsumerBillReceiptAsyncCommand
            {
                Service = facturalusaService.Value,
                Data = Convert(consumerBill),
                Format = DocumentFormat.EscPOS,
            });

            var result = ConvertToConsumerBill(document);
            return result;
        }

        public async Task<ConsumerBill> GetConsumerBillReceipt(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetSaleAsyncQuery
            {
                Service = facturalusaService.Value,
                DocumentId = documentId,
            });

            var result = ConvertToConsumerBill(document);
            return result;
        }

        public Task<byte[]> GetConsumerBillFile(string documentId, DocumentFileFormat format) => GetFileContent(documentId, format);
        #endregion

        #region Credit Note
        public async Task<CreditNote> CreateCreditNote(CreditNote creditNote)
        {
            var document = await CommandProcessor.Execute(new CreateCreditNoteAsyncCommand
            {
                Service = facturalusaService.Value,
                Data = Convert(creditNote),
                Format = DocumentFormat.POS,
            });

            return ConvertToCreditNote(document);
        }

        public async Task<CreditNote> GetCreditNote(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetSaleAsyncQuery
            {
                Service = facturalusaService.Value,
                DocumentId = documentId,
            });
            return ConvertToCreditNote(document);
        }

        public Task<byte[]> GetCreditNoteFile(string documentId, DocumentFileFormat format) => GetFileContent(documentId, format);
        #endregion


        #region Invoice Cancellation
        public async Task<InvoiceCancellation> CreateInvoiceCancellation(InvoiceCancellation invoiceCancellation)
        {
            var document = await CommandProcessor.Execute(new CreateInvoiceCancellationAsyncCommand
            {
                Service = facturalusaService.Value,
                Data = Convert(invoiceCancellation),
                Format = DocumentFormat.POS,
            });

            return ConvertToInvoiceCancellation(document);
        }
        #endregion

        #region Simplified Invoice
        public async Task<SimplifiedInvoice> CreateSimplifiedInvoice(SimplifiedInvoice invoice)
        {
            var document = await CommandProcessor.Execute(new CreateSimplifiedInvoiceAsyncCommand
            {
                Service = facturalusaService.Value,
                Data = Convert(invoice),
                Format = DocumentFormat.POS,
            });

            return ConvertToSimplifiedInvoice(document);
        }

        public async Task<SimplifiedInvoice> GetSimplifiedInvoice(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetSaleAsyncQuery
            {
                Service = facturalusaService.Value,
                DocumentId = documentId,
            });
            return ConvertToSimplifiedInvoice(document);
        }

        public Task<byte[]> GetSimplifiedInvoiceFile(string documentId, DocumentFileFormat format) => GetFileContent(documentId, format);
        #endregion

        public Task UpsertInvoiceItems(IEnumerable<ProductItem> items)
        {
            return CommandProcessor.Execute(new UpsertItemsAsyncCommand
            {
                Service = facturalusaService.Value,
                Items = items
                    .Select(it =>
                    {
                        var itemType = Convert(it.Type);
                        return new ItemData
                        {
                            Reference = it.Reference,
                            CorrelationId = it.CorrelationId,
                            Name = it.Name,
                            Details = it.Description,
                            Type = itemType.Type,
                            IsGenericItem = itemType.IsGeneric,
                            VatRatePercentage = it.TaxPercentage,
                            IsDeleted = it.IsDeleted,
                        };
                    })
                    .ToList(),
            });
        }

        #region Auxiliar Methods
        private static DateTime ConvertUtcToLocalDateTime(DateTime utcDatetime, string timeZoneId = "GMT Standard Time")
        {
            if (utcDatetime.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"The input datetime is not UTC DateKind. The current kind is '{utcDatetime.Kind}'");

            if (string.IsNullOrEmpty(timeZoneId))
                timeZoneId = "GMT Standard Time";
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDatetime, tzi);
        }

        private Task<byte[]> GetFileContent(string documentId, DocumentFileFormat format)
        {
            return QueryProcessor.Execute(new GetDocumentFileAsyncQuery
            {
                Service = facturalusaService.Value,
                DocumentId = documentId,
                DocumentFormat = Convert(format),
            });
        }
        #endregion

        #region Mappings
        private InvoiceCancellation ConvertToInvoiceCancellation(Sale src)
        {
            return new InvoiceCancellation
            {
                Customer = Convert(src.Customer),
                RelatedDocumentId = src.SaleReference?.DocumentFullNumber ?? throw new Exception("This should never happen"),
                PaymentMethodCode = src.PaymentMethod?.Description ?? throw new Exception("This should never happen"),
                SerieCode = src.Serie.Description,
                Items = src.Items.Select(Convert),
                DocumentId = src.DocumentFullNumber,
                CreatedDateUtc = src.IssueDate.ToUniversalTime(),
                Notes = src.Observations,
                PricesType = Map(src.VatType),
                Reason = src.CanceledReason!,
            };
        }

        private CreateInvoiceCancellationAsyncCommand.CreateInvoiceCancellationData Convert(InvoiceCancellation src)
        {
            return new CreateInvoiceCancellationAsyncCommand.CreateInvoiceCancellationData
            {
                DocumentId = src.RelatedDocumentId,
                Reason = src.Reason,
            };
        }

        private SimplifiedInvoice ConvertToSimplifiedInvoice(Sale src)
        {
            return new SimplifiedInvoice
            {
                CustomerVatNumber = src.CustomerVatNumber,
                PaymentMethodCode = src.PaymentMethod?.Description ?? throw new Exception("This should never happen"),
                SerieCode = src.Serie.Description,
                Items = src.Items.Select(Convert),
                DocumentId = src.DocumentFullNumber,
                CreatedDateUtc = src.IssueDate.ToUniversalTime(),
                Notes = src.Observations,
                PricesType = Map(src.VatType),
            };
        }

        private CreateSimplifiedInvoiceAsyncCommand.CreateSimplifiedInvoiceData Convert(SimplifiedInvoice src)
        {
            return new CreateSimplifiedInvoiceAsyncCommand.CreateSimplifiedInvoiceData
            {
                VatNumber = src.CustomerVatNumber,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
                Items = src.Items.Select(Convert),
                CreatedDateLocal = ConvertUtcToLocalDateTime(src.CreatedDateUtc),
                Observations = src.Notes,
                VatType = Map(src.PricesType),
                Reference = src.Reference,
            };
        }

        private CreditNote ConvertToCreditNote(Sale src)
        {
            return new CreditNote
            {
                Customer = Convert(src.Customer),
                RelatedDocumentId = src.SaleReference?.DocumentFullNumber ?? throw new Exception("This should never happen"),
                PaymentMethodCode = src.PaymentMethod?.Description ?? throw new Exception("This should never happen"),
                SerieCode = src.Serie.Description,
                Items = src.Items.Select(Convert),
                DocumentId = src.DocumentFullNumber,
                CreatedDateUtc = src.IssueDate.ToUniversalTime(),
                Notes = src.Observations,
                PricesType = Map(src.VatType),
            };
        }

        private CreateCreditNoteAsyncCommand.CreateCreditNoteData Convert(CreditNote src)
        {
            return new CreateCreditNoteAsyncCommand.CreateCreditNoteData
            {
                Customer = Convert(src.Customer),
                RelatedDocumentId = src.RelatedDocumentId,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
                Items = src.Items.Select(Convert),
                CreatedDateLocal = ConvertUtcToLocalDateTime(src.CreatedDateUtc),
                Observations = src.Notes,
                VatType = Map(src.PricesType),
            };
        }

        private ConsumerBill ConvertToConsumerBill(Sale src)
        {
            return new ConsumerBill
            {
                CustomerVatNumber = src.Customer.VatNumber,
                SerieCode = src.Serie.Description,
                Items = src.Items.Select(Convert),
                DocumentId = src.DocumentFullNumber,
                CreatedDateUtc = src.IssueDate.ToUniversalTime(),
                Notes = src.Observations,
                PricesType = Map(src.VatType),
            };
        }

        private DocumentFormat Convert(DocumentFileFormat format)
        {
            return format switch
            {
                DocumentFileFormat.EscPOS => DocumentFormat.EscPOS,
                DocumentFileFormat.A4 => DocumentFormat.A4,
                DocumentFileFormat.POS => DocumentFormat.POS,
                _ => throw new NotImplementedException(),
            };
        }

        private CreateInvoiceReceiptAsyncCommand.CreateInvoiceReceiptData Convert(InvoiceReceipt src)
        {
            return new CreateInvoiceReceiptAsyncCommand.CreateInvoiceReceiptData
            {
                Reference = src.Reference, //TODO: Make required
                Customer = Convert(src.Customer),
                SerieCode = src.SerieCode,
                PaymentMethodCode = src.PaymentMethodCode,
                Items = src.Items.Select(Convert),
                VatType = Map(src.PricesType),
                Observations = src.Notes,
                CreatedDateLocal = ConvertUtcToLocalDateTime(src.CreatedDateUtc),
            };
        }

        private CreateConsumerBillReceiptAsyncCommand.CreateConsumerBillData Convert(ConsumerBill consumerBill)
        {
            return new CreateConsumerBillReceiptAsyncCommand.CreateConsumerBillData
            {
                Reference = consumerBill.Reference, //TODO: Make required
                VatNumber = consumerBill.CustomerVatNumber,
                Items = consumerBill.Items.Select(Convert),
                SerieCode = consumerBill.SerieCode,
                CreatedDateLocal = ConvertUtcToLocalDateTime(consumerBill.CreatedDateUtc),
                Observations = consumerBill.Notes,
                VatType = Map(consumerBill.PricesType),
            };
        }

        private SaleDocumentItem Convert(InvoiceItem item)
        {
            var parsedType = Convert(item.Type);
            return new SaleDocumentItem
            {
                Reference = item.Reference, //TODO: Make required
                CorrelationId = item.CorrelationId,
                Name = item.Name,
                Price = item.Price,
                TaxPercentage = item.TaxPercentage,
                Description = item.Description, //TODO: Make required
                Quantity = item.Quantity,
                DiscountPercentage = item.DiscountPercentage,
                Type = parsedType.Type,
                IsGeneric = parsedType.IsGeneric,
            };
        }

        private (ItemType Type, bool IsGeneric) Convert(InvoiceItemType src)
        {
            switch (src)
            {
                case InvoiceItemType.Services:
                    return (ItemType.Service, false);
                case InvoiceItemType.Generic:
                    return (ItemType.Product, true);
                case InvoiceItemType.ProcessedProducts:
                default:
                    return (ItemType.Product, false);
            }
        }

        private SaleDocumentCustomer Convert(Infrastructure.Abstractions.Pos.Invoicing.Models.Customer src)
        {
            var parsedType = Convert(src.Type);
            return new SaleDocumentCustomer
            {
                Code = src.Code,
                Type = parsedType.Type,
                IsFinalConsumer = parsedType.IsFinalConsumer,
                Name = src.Name,
                VatNumber = src.VatNumber, // TODO: This should be required
                Email = src.Email,
                MobileNumber = src.MobileNumber,
                Address = src.Address,
                PostalCode = src.PostalCode,
                City = src.CityName,
                Country = src.CountryName,
            };
        }

        private (global::FacturaLusa.v2.Dtos.CustomerType Type, bool IsFinalConsumer) Convert(Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType type)
        {
            switch (type)
            {
                case Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.Company:
                    return (global::FacturaLusa.v2.Dtos.CustomerType.Business, false);
                case Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.FinalConsumer:
                    return (global::FacturaLusa.v2.Dtos.CustomerType.Personal, true);
                case Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.Personal:
                default:
                    return (global::FacturaLusa.v2.Dtos.CustomerType.Personal, false);
            }
        }

        private VatType Map(PriceType pricesType)
        {
            return pricesType switch
            {
                PriceType.IncludedTaxes => VatType.VatIncluded,
                PriceType.NotIncludedTaxes => VatType.VatDebit,
                _ => throw new NotImplementedException(),
            };
        }

        private PriceType Map(VatType pricesType)
        {
            return pricesType switch
            {
                VatType.VatIncluded => PriceType.IncludedTaxes,
                VatType.VatDebit => PriceType.NotIncludedTaxes,
                _ => throw new NotImplementedException(),
            };
        }

        private InvoiceReceipt ConvertToInvoiceReceipt(Sale src)
        {
            var dest = new InvoiceReceipt
            {
                SerieCode = src.Serie.Description,
                PaymentMethodCode = src.PaymentMethod?.Description ?? throw new Exception("This should never happen"),
                Customer = Convert(src.Customer),
                DocumentId = src.DocumentFullNumber,
                CreatedDateUtc = src.IssueDate.ToUniversalTime(),
                Notes = src.Observations,
                PricesType = Map(src.VatType),
                Items = src.Items.Select(Convert),
            };
            return dest;
        }

        private InvoiceItem Convert(SaleItem item)
        {
            var type = Convert(item.Item.Type, item.Item.Description == "Diversos");
            return new InvoiceItem(type)
            {
                Reference = item.Item.Reference,
                CorrelationId = item.Item.Reference ?? string.Empty,
                Name = item.Item.Description ?? string.Empty,
                Price = item.UnitPrice,
                Quantity = item.Quantity,
                TaxPercentage = item.VatRate.TaxPercentage,
                Description = item.Item.Description,
                DiscountPercentage = 0m, //TODO: Check if correct
            };
        }

        private InvoiceItemType Convert(ItemType src, bool isGeneric)
        {
            if (isGeneric)
                return InvoiceItemType.Generic;

            switch (src)
            {
                case ItemType.Service:
                    return InvoiceItemType.Services;
                case ItemType.Product:
                default: return InvoiceItemType.ProcessedProducts;
            }
        }

        private Infrastructure.Abstractions.Pos.Invoicing.Models.Customer Convert(SaleCustomer src)
        {
            var type = Convert(src.Type, false); //TODO: Check
            return new Infrastructure.Abstractions.Pos.Invoicing.Models.Customer(type)
            {
                Code = src.Code,
                Name = src.Name,
                VatNumber = src.VatNumber,
                Email = src.Email,
                MobileNumber = src.MobilePhone,
                Address = src.Address,
                PostalCode = src.PostalCode,
                CityName = src.City,
                CountryName = src.Country,
            };
        }

        private Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType Convert(global::FacturaLusa.v2.Dtos.CustomerType type, bool isFinalConsumer)
        {
            if (isFinalConsumer)
                return Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.FinalConsumer;

            switch (type)
            {
                case global::FacturaLusa.v2.Dtos.CustomerType.Business:
                    return Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.Company;
                case global::FacturaLusa.v2.Dtos.CustomerType.Personal:
                default:
                    return Infrastructure.Abstractions.Pos.Invoicing.Models.CustomerType.Personal;
            }
        }
        #endregion
    }
}
