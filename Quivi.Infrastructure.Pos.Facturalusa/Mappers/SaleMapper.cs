using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.External;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Sales;
using Quivi.Infrastructure.Pos.Facturalusa.Models.VatRates;

namespace Quivi.Infrastructure.Pos.Facturalusa.Mappers
{
    internal static class SaleMapper
    {
        public static CreditNote ConvertToCreditNote(ReadonlySale sale)
        {
            var creditNote = new CreditNote
            {
                Customer = new Customer(),
                RelatedDocumentId = sale.ReferencedSale?.ComposedNumber ?? string.Empty,
                Items = sale.Items.Select(Convert),
                PaymentMethodCode = string.Empty,
                SerieCode = sale.Serie?.Name ?? string.Empty,
            };
            Map(sale, creditNote);
            Map(sale.Customer!, creditNote.Customer);
            return creditNote;
        }
        
        public static InvoiceCancellation ConvertToInvoiceCancellation(CancelSaleResponse sale)
        {
            var invoiceCancellation = new InvoiceCancellation
            {
                UrlFile = sale.UrlFile,
                Customer = new Customer(),
                Items = [],
                PaymentMethodCode = string.Empty,
                RelatedDocumentId = string.Empty,
                SerieCode = string.Empty,
            };
            return invoiceCancellation;
        }

        public static InvoiceReceipt ConvertToInvoiceReceipt(ReadonlySale sale)
        {
            var invoiceReceipt = new InvoiceReceipt
            {
                Customer = new Customer(),
                Items = [],
                PaymentMethodCode = string.Empty,
                SerieCode = string.Empty,
            };
            Map(sale, invoiceReceipt);
            Map(sale.Customer!, invoiceReceipt.Customer);
            return invoiceReceipt;
        }

        public static SimplifiedInvoice ConvertToSimpliedInvoice(ReadonlySale sale)
        {
            var simplifiedInvoice = new SimplifiedInvoice
            {
                CustomerVatNumber = sale.Customer.VatNumber,
                Items = [],
                PaymentMethodCode = string.Empty,
                SerieCode = string.Empty,
            };
            Map(sale, simplifiedInvoice);
            return simplifiedInvoice;
        }
        
        public static ConsumerBill ConvertToBudgetReceipt(ReadonlySale sale)
        {
            var budgetReceipt = new ConsumerBill
            {
                CustomerVatNumber = sale.Customer?.VatNumber ?? string.Empty,
                Items = [],
                PaymentMethodCode = string.Empty,
                SerieCode = string.Empty,
            };
            Map(sale, budgetReceipt);
            return budgetReceipt;
        }
        
        private static void Map(ReadonlySale src, ADocument dest)
        {
            dest.DocumentId = src.ComposedNumber;
            dest.CreatedDateLocal = src.IssueDay;
            dest.PaymentMethodCode = src.PaymentMethod?.Name ?? string.Empty;
            dest.SerieCode = src.Serie?.Name ?? string.Empty;
            dest.PricesType = Convert(src.VatType);
            dest.Items = src.Items!.Select(Convert);
        }

        private static InvoiceItem Convert(ReadonlySaleItem item)
        {
            return new InvoiceItem(item.Details!.Type)
            {
                Reference = item.Details?.Reference ?? string.Empty,
                Name = item.Details?.Name ?? string.Empty,
                Description = item.Description,
                Price = item.Price,
                Quantity = item.Quantity,
                TaxPercentage = item.VatRate.PercentageTax,
                DiscountPercentage = item.DiscountPercentage,
            };
        }

        private static void Map(ReadOnlyCustomer src, Customer dest)
        {
            dest.Code = src.Code;
            dest.Name = src.Name;
            dest.VatNumber = src.VatNumber;
            dest.Email = src.Email;
            dest.MobileNumber = src.MobileNumber;
            dest.CountryName = src.CountryName;
            dest.CityName = src.CityName;
            dest.PostalCode = src.PostalCode;
            dest.Address = src.Address;
        }

        public static VatRateType Convert(PriceType type)
        {
            switch (type)
            {
                case PriceType.NotIncludedTaxes: 
                    return VatRateType.DebitVAT;
                case PriceType.IncludedTaxes:
                default:
                    return VatRateType.IncludedVAT;
            }
        }

        public static PriceType Convert(VatRateType type) 
        {
            switch (type)
            {
                case VatRateType.DebitVAT: 
                    return PriceType.NotIncludedTaxes;
                case VatRateType.IncludedVAT:
                default:
                    return PriceType.IncludedTaxes;
            }
        }
    }
}
