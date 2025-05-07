namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    /// <summary>
    /// Creates a named distributed lock related to the provided values.
    /// </summary>
    public class PerIntegrationDistributedLockFilter : NamedDistributedLockFilterAttribute
    {
        public PerIntegrationDistributedLockFilter() : base("Integration", (j) => j.PosIntegrationId)
        {
        }
    }
}
