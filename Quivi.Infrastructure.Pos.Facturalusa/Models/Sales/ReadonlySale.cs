using Newtonsoft.Json;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Currencies;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Customers;
using Quivi.Infrastructure.Pos.Facturalusa.Models.DocumentTypes;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentConditions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.PaymentMethods;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Series;

namespace Quivi.Infrastructure.Pos.Facturalusa.Models.Sales
{
    public class ReadonlySale : ASale
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("reference")]
        public ReadonlyReferencedSale? ReferencedSale { get; set; }

        [JsonProperty("serie", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ReadonlySerie? Serie { get; set; }

        [JsonProperty("documenttype", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DocumentTypeData? DocumentTypeData { get; set; }

        [JsonProperty("documenttypeserie", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DocumentNumberData? DocumentNumberData { get; set; }

        [JsonProperty("customer", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ReadOnlyCustomer? Customer { get; set; }

        [JsonProperty("currency", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Currency? Currency { get; set; }

        [JsonProperty("paymentmethod", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public ReadonlyPaymentMethod? PaymentMethod { get; set; }

        [JsonProperty("paymentcondition", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public PaymentCondition? PaymentCondition { get; set; }

        [JsonProperty("items", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public required IEnumerable<ReadonlySaleItem> Items { get; set; }

        [JsonProperty("url_file", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public required string PdfFileUrl { get; set; }

        [JsonProperty("file_format", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DownloadSaleFormat PdfFileFormat { get; set; }
        
        [JsonIgnore]
        public string? ComposedNumber
        {
            get
            {
                if (string.IsNullOrEmpty(DocumentTypeData?.Code) || string.IsNullOrEmpty(Serie?.Name) || DocumentNumberData == null)
                    return null;

                return $"{DocumentTypeData.Code} {Serie.Name}/{DocumentNumberData.Number}";
            }
        }
    }
}
