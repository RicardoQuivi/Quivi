namespace Quivi.Domain.Entities.Merchants
{
    public enum MerchantServiceType
    {
        /// <summary>
        /// Funds being charged to merchant and received by Quivi.
        /// VAT should be applied when charging funds.
        /// </summary>
        Billing = 0,

        /// <summary>
        /// Funds takend from Quivi and sent to merchant.
        /// </summary>
        Reimbursement = 1,
    }
}
