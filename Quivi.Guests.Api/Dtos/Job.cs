namespace Quivi.Guests.Api.Dtos
{
    public enum JobState
    {
        Failed = -1,
        AwaitingCompletion = 0,
        Completed = 1,
    }

    public class Job
    {
        public required string Id { get; init; }
        public JobState State { get; init; }
    }
}
