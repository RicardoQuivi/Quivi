namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    /// <summary>
    /// Creates a named distributed lock related to the provided values.
    /// </summary>
    public class PerMerchantDistributedLockFilter : NamedDistributedLockFilterAttribute
    {
        public PerMerchantDistributedLockFilter() : base("Merchant", (j) => j.MerchantId)
        {
        }
    }
}
