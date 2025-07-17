using Hangfire.States;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Pos.Api.Dtos;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class JobStateMapperHandler : IMapperHandler<string, JobState>
    {
        public JobState Map(string model)
        {
            bool isSucceeded = model == SucceededState.StateName;
            bool isFailed = model == FailedState.StateName;

            JobState state = JobState.AwaitingCompletion;
            if (isSucceeded)
                state = JobState.Completed;
            else if (isFailed)
                state = JobState.Failed;

            return state;
        }
    }
}
