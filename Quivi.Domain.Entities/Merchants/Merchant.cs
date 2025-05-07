using Quivi.Domain.Entities.Financing;
using Quivi.Domain.Entities.Identity;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;

namespace Quivi.Domain.Entities.Merchants
{
    public class Merchant : IDeletableEntity
    {
        public int Id { get; set; }

        public string? FiscalName { get; set; }
        public required string Name { get; set; }
        public string? Iban { get; set; }
        public string? VatNumber { get; set; }
        public string? LogoUrl { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? TimeZone { get; set; }
        public decimal? VatRate { get; set; }
        public string? IbanProofUrl { get; set; }
        public bool IsDemo { get; set; }
        public DateTime? TermsAndConditionsAcceptedDate { get; set; }
        public MerchantFeature DisabledFeatures { get; set; } //TODO: Check default

        public decimal TransactionFee { get; set; }
        public FeeUnit TransactionFeeUnit { get; set; }

        public decimal SurchargeFee { get; set; }
        public FeeUnit SurchargeFeeUnit { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int? ParentMerchantId { get; set; }
        public Merchant? ParentMerchant { get; set; }

        public int? SetUpFeeId { get; set; }
        public MerchantService? SetUpFee { get; set; }

        public ICollection<MerchantService> ?MerchantServices { get; set; }
        public ICollection<ApiClient>? ApiClients { get; set; }
        public ICollection<Person>? People { get; set; }
        public ICollection<Merchant>? ChildMerchants { get; set; }
        public ICollection<MerchantFee>? Fees { get; set; }
        //TODO: Migrate this or add it to MerchantInvoiceDocument?
        //public ICollection<MerchantInvoice> MerchantInvoices { get; set; }
        public ICollection<MerchantFile>? Files { get; set; }
        public ICollection<PreparationGroup>? PreparationGroups { get; set; }
        public ICollection<Location>? Locations { get; set; }
        public ICollection<NotificationsContact>? NotificationContacts { get; set; }
        public ICollection<MerchantInvoiceDocument>? InvoiceDocuments { get; set; }
        #endregion

        #region PoS Merchant Relationships
        public ICollection<PosIntegration>? PosIntegrations { get; set; }
        public ICollection<MenuItem>? MenuItems { get; set; }
        public ICollection<Channel>? Channels { get; set; }
        
        //TODO: Keep the following?
        //public ICollection<PostCheckoutLink> PostCheckoutLinks { get; set; }
        //public ICollection<PostCheckoutText> PostCheckoutTexts { get; set; }

        //TODO: Keep the following?
        //public ICollection<Featured> Featureds { get; set; }

        public ICollection<CustomChargeMethod>? CustomChargeMethods { get; set; }
        public ICollection<PosNotificationMessage>? PosNotificationMessages { get; set; }
        public ICollection<ChannelProfile>? ChannelProfiles { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<PrinterWorker>? PrinterWorkers { get; set; }
        #endregion

        public IEnumerable<MerchantFee>? TransactionFees => Fees?.Where(f => f.FeeType == FeeType.Transaction);
        public IEnumerable<MerchantFee>? SurchargeFees => Fees?.Where(f => f.FeeType == FeeType.Surcharge);
    }
}
