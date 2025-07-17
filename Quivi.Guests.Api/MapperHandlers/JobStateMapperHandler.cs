using Hangfire.States;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class JobStateMapperHandler : IMapperHandler<string, Dtos.JobState>
    {
        public Dtos.JobState Map(string model)
        {
            bool isSucceeded = model == SucceededState.StateName;
            bool isFailed = model == FailedState.StateName;

            Dtos.JobState state = Dtos.JobState.AwaitingCompletion;
            if (isSucceeded)
                state = Dtos.JobState.Completed;
            else if (isFailed)
                state = Dtos.JobState.Failed;

            return state;
        }
    }
}