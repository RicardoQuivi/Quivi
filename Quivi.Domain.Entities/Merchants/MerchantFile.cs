namespace Quivi.Domain.Entities.Merchants
{
    public class MerchantFile : IDeletableEntity
    {
        public int MerchantFileId { get; set; }

        public required string FileUrl { get; set; }
        public required string FileMetadata { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }

        #region Relationships
        public int MerchantId { get; set; }
        public required Merchant Merchant { get; set; }
        #endregion
    }
}
