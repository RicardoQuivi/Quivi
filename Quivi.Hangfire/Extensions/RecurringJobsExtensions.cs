using Hangfire;
using Quivi.Hangfire.Hangfire;

namespace Quivi.Hangfire.Extensions
{
    public static class RecurringJobsExtensions
    {
        public static async Task AddRecurringJobs(this IServiceProvider serviceProvider)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var recurringJobs = services.GetService<IEnumerable<IRecurringJob>>() ?? [];

                foreach (var job in recurringJobs)
                {
                    string jobId = job.GetType().Name;
                    RecurringJob.AddOrUpdate(jobId, () => job.Run(), job.Schedule);
                }
            }
        }
    }
}