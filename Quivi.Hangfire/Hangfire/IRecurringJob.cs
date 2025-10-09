namespace Quivi.Hangfire.Hangfire
{
    public interface IRecurringJob
    {
        /// <summary>
        /// How frequency the job runs.
        /// Please use the helper <see cref="Hangfire.Cron"/> to set this field.
        /// </summary>
        string Schedule { get; }

        /// <summary>
        /// The method that is called when the job runs
        /// </summary>
        Task Run();
    }
}