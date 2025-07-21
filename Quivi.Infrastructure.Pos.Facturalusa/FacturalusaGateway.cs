using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.Invoicing;
using Quivi.Infrastructure.Pos.Facturalusa.Abstractions;
using Quivi.Infrastructure.Pos.Facturalusa.Commands;
using Quivi.Infrastructure.Pos.Facturalusa.Queries;
using facturalusaModels = Quivi.Infrastructure.Pos.Facturalusa.Models.External;
using gatewayAbstractions = Quivi.Infrastructure.Abstractions.Pos.Invoicing.Models;

namespace Quivi.Infrastructure.Pos.Facturalusa
{
    public class FacturalusaGateway : IInvoiceGateway
    {
        public required IQueryProcessor QueryProcessor { get; init; }
        public required ICommandProcessor CommandProcessor { get; init; }
        private readonly Lazy<IFacturalusaService> facturalusaService;

        public string GatewayCode => "FL";

        public FacturalusaGateway(IFacturalusaServiceFactory facturalusaServiceFactory, string accessToken, string accountUuid)
        {
            facturalusaService = new Lazy<IFacturalusaService>(() => facturalusaServiceFactory.Create(accessToken, accountUuid));
        }

        #region Invoice Receipt
        public async Task<gatewayAbstractions.InvoiceReceipt> CreateInvoiceReceipt(gatewayAbstractions.InvoiceReceipt invoice)
        {
            var readDocument = await CommandProcessor.Execute(new CreateInvoiceReceiptAsyncCommand(facturalusaService.Value)
            {
                Data = Convert(invoice),
                IncludePdfFileInFormat = Models.Sales.DownloadSaleFormat.POS,
            });

            return Convert(readDocument);
        }

        public async Task<gatewayAbstractions.InvoiceReceipt> GetInvoiceReceipt(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetInvoiceReceiptAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
            });

            return Convert(document);
        }

        public Task<byte[]> GetInvoiceReceiptFile(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileContent(documentId, format);

        public Task<string> GetInvoiceReceiptFileUrl(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileUrl(documentId, format);

        private facturalusaModels.InvoiceReceipt Convert(gatewayAbstractions.InvoiceReceipt src)
        {
            var dest = new facturalusaModels.InvoiceReceipt
            {
                Customer = Convert(src.Customer),
                SerieCode = src.SerieCode,
                PaymentMethodCode = src.PaymentMethodCode,
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        private gatewayAbstractions.InvoiceReceipt Convert(facturalusaModels.InvoiceReceipt src)
        {
            var dest = new gatewayAbstractions.InvoiceReceipt
            {
                SerieCode = src.SerieCode,
                PaymentMethodCode = src.PaymentMethodCode,
                Customer = Convert(src.Customer),
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        #endregion

        #region Consumer Bill

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.ConsumerBill> CreateConsumerBillReceipt(gatewayAbstractions.ConsumerBill receipt)
        {
            var readDocument = await CommandProcessor.Execute(new CreateConsumerBillReceiptAsyncCommand(facturalusaService.Value)
            {
                Data = Convert(receipt),
                IncludePdfFileInFormat = Models.Sales.DownloadSaleFormat.EscPOS,
            });

            return Convert(readDocument);
        }

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.ConsumerBill> GetConsumerBillReceipt(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetConsumerBillReceiptAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
            });

            return Convert(document);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetConsumerBillFile(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileContent(documentId, format);

        /// <inheritdoc/>
        public Task<string> GetConsumerBillFileUrl(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileUrl(documentId, format);

        private facturalusaModels.ConsumerBill Convert(gatewayAbstractions.ConsumerBill src)
        {
            var dest = new facturalusaModels.ConsumerBill
            {
                CustomerVatNumber = src.CustomerVatNumber,
                Items = src.Items.Select(Convert),
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
            };
            Map(src, dest);
            return dest;
        }

        private gatewayAbstractions.ConsumerBill Convert(facturalusaModels.ConsumerBill src)
        {
            var dest = new gatewayAbstractions.ConsumerBill
            {
                CustomerVatNumber = src.CustomerVatNumber,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        #endregion

        #region Credit Note

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.CreditNote> CreateCreditNote(gatewayAbstractions.CreditNote creditNote)
        {
            var readDocument = await CommandProcessor.Execute(new CreateCreditNoteAsyncCommand(facturalusaService.Value)
            {
                Data = Convert(creditNote),
                IncludePdfFileInFormat = Models.Sales.DownloadSaleFormat.POS,
            });

            return Convert(readDocument);
        }

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.CreditNote> GetCreditNote(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetCreditNoteAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
            });

            return Convert(document);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetCreditNoteFile(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileContent(documentId, format);

        /// <inheritdoc/>
        public Task<string> GetCreditNoteFileUrl(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileUrl(documentId, format);

        private facturalusaModels.CreditNote Convert(gatewayAbstractions.CreditNote src)
        {
            var dest = new facturalusaModels.CreditNote
            {
                Customer = Convert(src.Customer),
                RelatedDocumentId = src.RelatedDocumentId,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        private gatewayAbstractions.CreditNote Convert(facturalusaModels.CreditNote src)
        {
            var dest = new gatewayAbstractions.CreditNote
            {
                Customer = Convert(src.Customer),
                RelatedDocumentId = src.RelatedDocumentId,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
                Items = src.Items.Select(Convert)
            };
            Map(src, dest);
            return dest;
        }

        #endregion

        #region Invoice Cancellation

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.InvoiceCancellation> CreateInvoiceCancellation(gatewayAbstractions.InvoiceCancellation invoiceCancellation)
        {
            var readDocument = await CommandProcessor.Execute(new CreateInvoiceCancellationAsyncCommand(facturalusaService.Value)
            {
                Data = Convert(invoiceCancellation),
                IncludePdfFileInFormat = Models.Sales.DownloadSaleFormat.POS,
            });

            return Convert(readDocument);
        }

        private facturalusaModels.InvoiceCancellation Convert(gatewayAbstractions.InvoiceCancellation src)
        {
            var dest = new facturalusaModels.InvoiceCancellation
            {
                RelatedDocumentId = src.RelatedDocumentId,
                Reason = src.Reason,
                Customer = Convert(src.Customer),
                Items = src.Items.Select(Convert),
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
            };
            return dest;
        }

        private gatewayAbstractions.InvoiceCancellation Convert(facturalusaModels.InvoiceCancellation src)
        {
            var dest = new gatewayAbstractions.InvoiceCancellation
            {
                RelatedDocumentId = src.RelatedDocumentId,
                Reason = src.Reason,
                Customer = Convert(src.Customer),
                Items = src.Items.Select(Convert),
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.SerieCode,
            };
            Map(src, dest);
            return dest;
        }

        #endregion

        #region Simplified Invoice

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.SimplifiedInvoice> CreateSimplifiedInvoice(gatewayAbstractions.SimplifiedInvoice invoice)
        {
            var readDocument = await CommandProcessor.Execute(new CreateSimplifiedInvoiceAsyncCommand(facturalusaService.Value)
            {
                Data = Convert(invoice),
                IncludePdfFileInFormat = Models.Sales.DownloadSaleFormat.POS,
            });

            return Convert(readDocument);
        }

        /// <inheritdoc/>
        public async Task<gatewayAbstractions.SimplifiedInvoice> GetSimplifiedInvoice(string documentId)
        {
            var document = await QueryProcessor.Execute(new GetSimplifiedInvoiceAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
            });

            return Convert(document);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetSimplifiedInvoiceFile(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileContent(documentId, format);

        /// <inheritdoc/>
        public Task<string> GetSimplifiedInvoiceFileUrl(string documentId, gatewayAbstractions.DocumentFileFormat format) => GetFileUrl(documentId, format);

        private facturalusaModels.SimplifiedInvoice Convert(gatewayAbstractions.SimplifiedInvoice src)
        {
            var dest = new facturalusaModels.SimplifiedInvoice
            {
                CustomerVatNumber = src.CustomerVatNumber,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.PaymentMethodCode,
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        private gatewayAbstractions.SimplifiedInvoice Convert(facturalusaModels.SimplifiedInvoice src)
        {
            var dest = new gatewayAbstractions.SimplifiedInvoice
            {
                CustomerVatNumber = src.CustomerVatNumber,
                PaymentMethodCode = src.PaymentMethodCode,
                SerieCode = src.PaymentMethodCode,
                Items = src.Items.Select(Convert),
            };
            Map(src, dest);
            return dest;
        }

        #endregion

        #region Auxiliar Methods

        private async Task<byte[]> GetFileContent(string documentId, gatewayAbstractions.DocumentFileFormat format)
        {
            return await QueryProcessor.Execute(new GetDocumentFileAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
                DocumentFormat = Convert(format),
            });
        }

        private async Task<string> GetFileUrl(string documentId, gatewayAbstractions.DocumentFileFormat format)
        {
            return await QueryProcessor.Execute(new GetDocumentFileUrlAsyncQuery(facturalusaService.Value)
            {
                DocumentId = documentId,
                DocumentFormat = Convert(format),
            });
        }

        #endregion

        #region Converters Facturalusa -> Gateway

        private void Map(facturalusaModels.ADocument src, gatewayAbstractions.ADocument dest)
        {
            dest.DocumentId = src.DocumentId;
            dest.CreatedDateUtc = src.CreatedDateLocal.ToUniversalTime();
            dest.SerieCode = src.SerieCode;
            dest.PaymentMethodCode = src.PaymentMethodCode;
            dest.Notes = src.Notes;
            dest.PricesType = (gatewayAbstractions.PriceType)src.PricesType;
            dest.Items = src.Items.Select(Convert);
        }

        private gatewayAbstractions.InvoiceItem Convert(facturalusaModels.InvoiceItem item)
        {
            var type = Convert(item.Type, item.IsGeneric);
            return new gatewayAbstractions.InvoiceItem(type)
            {
                Reference = item.Reference,
                CorrelationId = item.CorrelationId ?? string.Empty,
                Name = item.Name ?? string.Empty,
                Price = item.Price,
                Quantity = item.Quantity,
                TaxPercentage = item.TaxPercentage,
                Description = item.Description,
                DiscountPercentage = item.DiscountPercentage,
            };
        }

        private facturalusaModels.Customer Convert(gatewayAbstractions.Customer src)
        {
            var parsedType = Convert(src.Type);
            return new facturalusaModels.Customer
            {
                Code = src.Code,
                Type = parsedType.Type,
                IsFinalConsumer = parsedType.IsFinalConsumer,
                Name = src.Name,
                VatNumber = src.VatNumber,
                Email = src.Email,
                MobileNumber = src.MobileNumber,
                Address = src.Address,
                PostalCode = src.PostalCode,
                CityName = src.CityName,
                CountryName = src.CountryName,
            };
        }

        private gatewayAbstractions.InvoiceItemType Convert(Models.Items.ItemType src, bool isGeneric)
        {
            if (isGeneric)
                return gatewayAbstractions.InvoiceItemType.Generic;

            switch (src)
            {
                case Models.Items.ItemType.Services:
                    return gatewayAbstractions.InvoiceItemType.Services;
                case Models.Items.ItemType.FinishAndIntermediateProducts:
                default:
                    return gatewayAbstractions.InvoiceItemType.ProcessedProducts;
            }
        }

        private gatewayAbstractions.CustomerType Convert(Models.Customers.CustomerType type, bool isFinalConsumer)
        {
            if (isFinalConsumer)
                return gatewayAbstractions.CustomerType.FinalConsumer;

            switch (type)
            {
                case Models.Customers.CustomerType.Company:
                    return gatewayAbstractions.CustomerType.Company;
                case Models.Customers.CustomerType.Personal:
                default:
                    return gatewayAbstractions.CustomerType.Personal;
            }
        }

        #endregion

        #region Converters Gateway -> Facturalusa
        private DateTime ConvertUtcToLocalDateTime(DateTime utcDatetime, string timeZoneId = "GMT Standard Time")
        {
            if (utcDatetime.Kind != DateTimeKind.Utc)
                throw new ArgumentException($"The input datetime is not UTC DateKind. The current kind is '{utcDatetime.Kind}'");

            if (string.IsNullOrEmpty(timeZoneId))
                timeZoneId = "GMT Standard Time";
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDatetime, tzi);
        }

        private void Map(gatewayAbstractions.ADocument src, facturalusaModels.ADocument dest)
        {
            dest.DocumentId = src.DocumentId;
            dest.CreatedDateLocal = ConvertUtcToLocalDateTime(src.CreatedDateUtc);
            dest.SerieCode = src.SerieCode;
            dest.PaymentMethodCode = src.PaymentMethodCode;
            dest.Notes = src.Notes;
            dest.PricesType = (facturalusaModels.PriceType)src.PricesType;
        }

        private facturalusaModels.InvoiceItem Convert(gatewayAbstractions.BaseItem item)
        {
            var parsedType = Convert(item.Type);
            return new facturalusaModels.InvoiceItem(parsedType.Type, parsedType.IsGeneric)
            {
                Reference = item.Reference,
                CorrelationId = item.CorrelationId,
                Name = item.Name,
                Price = item.Price,
                TaxPercentage = item.TaxPercentage,
                Description = item.Description,
            };
        }

        private facturalusaModels.InvoiceItem Convert(gatewayAbstractions.InvoiceItem item)
        {
            var baseItem = Convert((gatewayAbstractions.BaseItem)item);
            baseItem.Quantity = item.Quantity;
            baseItem.DiscountPercentage = item.DiscountPercentage;
            return baseItem;
        }

        private gatewayAbstractions.Customer Convert(facturalusaModels.Customer src)
        {
            var type = Convert(src.Type, src.IsFinalConsumer);
            return new gatewayAbstractions.Customer(type)
            {
                Code = src.Code,
                Name = src.Name,
                VatNumber = src.VatNumber,
                Email = src.Email,
                MobileNumber = src.MobileNumber,
                Address = src.Address,
                PostalCode = src.PostalCode,
                CityName = src.CityName,
                CountryName = src.CountryName,
            };
        }

        private (Models.Items.ItemType Type, bool IsGeneric) Convert(gatewayAbstractions.InvoiceItemType src)
        {
            switch (src)
            {
                case gatewayAbstractions.InvoiceItemType.Services:
                    return (Models.Items.ItemType.Services, false);
                case gatewayAbstractions.InvoiceItemType.Generic:
                    return (Models.Items.ItemType.FinishAndIntermediateProducts, true);
                case gatewayAbstractions.InvoiceItemType.ProcessedProducts:
                default:
                    return (Models.Items.ItemType.FinishAndIntermediateProducts, false);
            }
        }

        private (Models.Customers.CustomerType Type, bool IsFinalConsumer) Convert(gatewayAbstractions.CustomerType type)
        {
            switch (type)
            {
                case gatewayAbstractions.CustomerType.Company:
                    return (Models.Customers.CustomerType.Company, false);
                case gatewayAbstractions.CustomerType.FinalConsumer:
                    return (Models.Customers.CustomerType.Personal, true);
                case gatewayAbstractions.CustomerType.Personal:
                default:
                    return (Models.Customers.CustomerType.Personal, false);
            }
        }

        private Models.Sales.DownloadSaleFormat Convert(gatewayAbstractions.DocumentFileFormat src)
        {
            switch (src)
            {
                case gatewayAbstractions.DocumentFileFormat.POS:
                    return Models.Sales.DownloadSaleFormat.POS;
                case gatewayAbstractions.DocumentFileFormat.EscPOS:
                    return Models.Sales.DownloadSaleFormat.EscPOS;
                case gatewayAbstractions.DocumentFileFormat.A4:
                default:
                    return Models.Sales.DownloadSaleFormat.A4;
            }
        }

        public async Task UpsertInvoiceItems(IEnumerable<gatewayAbstractions.ProductItem> items)
        {
            await CommandProcessor.Execute(new UpsertItemsAsyncCommand(facturalusaService.Value)
            {
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

        public async Task<bool> HealthCheck() => await QueryProcessor.Execute(new HealthCheckQuery(facturalusaService.Value));
        #endregion
    }
}