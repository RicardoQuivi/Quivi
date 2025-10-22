using Quivi.Domain.Entities.Merchants;

namespace Quivi.Domain.Entities.Identity
{
    public class ApiClientRequest : IEntity
    {
        public int ApiClientRequestId { get; set; }

        public string? RedeemCode { get; set; }
        public string? DeviceReference { get; set; }
        public ApiClientRequestStatus Status { get; set; }

        public DateTime CreatedDate { get; set; } //TODO: Removed Default Value
        public DateTime ModifiedDate { get; set; } //TODO: Removed Default Value
        public DateTime ExpirationDate { get; set; }

        public ApiClientRequestSource Source { get; set; }

        #region Relationships
        public int SubMerchantId { get; set; }
        public required Merchant SubMerchant { get; set; }

        public int? ApiClientId { get; set; }
        public ApiClient? ApiClient { get; set; }
        #endregion
    }

    //TODO: Am I still needed
    public enum ApiClientRequestSource
    {
        /// <summary>
        /// Page inside the app.
        /// </summary>
        Page = 0,

        /// <summary>
        /// External URL without authorization.
        /// </summary>
        Url = 1,

        /// <summary>
        /// Generates and Redeem code automatically to generate a token.
        /// </summary>
        Automatic = 2,
    }

    public enum ApiClientRequestStatus
    {
        Created = 0,
        Used = 1,
        Expired = 2,
    }
}
