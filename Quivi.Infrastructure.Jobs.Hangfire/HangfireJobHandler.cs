using Hangfire;
using Quivi.Infrastructure.Abstractions.Jobs;
using System.Linq.Expressions;

namespace Quivi.Infrastructure.Jobs.Hangfire
{
    public class HangfireJobHandler : IBackgroundJobHandler
    {
        private readonly IBackgroundJobClient backgroundJobClient;

        public HangfireJobHandler(IBackgroundJobClient backgroundJobClient)
        {
            this.backgroundJobClient = backgroundJobClient;
        }

        private JobContinuationOptions Map(JobContinuation continuation)
        {
            switch (continuation)
            {
                case JobContinuation.OnFinished: return JobContinuationOptions.OnAnyFinishedState;
                case JobContinuation.OnSuccessfullyFinished: return JobContinuationOptions.OnlyOnSucceededState;
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        public string ContinueJobWith(string parentId, Expression<Action> methodCall) => backgroundJobClient.ContinueJobWith(parentId, methodCall);
        public string ContinueJobWith<T>(string parentId, Expression<Action<T>> methodCall) => backgroundJobClient.ContinueJobWith(parentId, methodCall);
        public string ContinueJobWith(string parentId, Expression<Func<Task>> methodCall) => backgroundJobClient.ContinueJobWith(parentId, methodCall);
        public string ContinueJobWith(string parentId, Expression<Func<Task>> methodCall, JobContinuation continuation) => backgroundJobClient.ContinueJobWith(parentId, methodCall, null, Map(continuation));
        public string Enqueue(Expression<Action> methodCall) => backgroundJobClient.Enqueue(methodCall);
        public string Enqueue(Expression<Func<Task>> methodCall) => backgroundJobClient.Enqueue(methodCall);
        public string Enqueue<T>(Expression<Action<T>> methodCall) => backgroundJobClient.Enqueue(methodCall);
        public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => backgroundJobClient.Enqueue(methodCall);
        public string Schedule(Expression<Action> methodCall, TimeSpan delay) => backgroundJobClient.Schedule(methodCall, delay);
        public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) => backgroundJobClient.Schedule(methodCall, delay);
        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) => backgroundJobClient.Schedule(methodCall, delay);
        public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => backgroundJobClient.Schedule(methodCall, delay);
        public string Schedule(Expression<Action> methodCall, DateTimeOffset atDate) => backgroundJobClient.Schedule(methodCall, atDate);
        public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset atDate) => backgroundJobClient.Schedule(methodCall, atDate);
        public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset atDate) => backgroundJobClient.Schedule(methodCall, atDate);
        public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset atDate) => backgroundJobClient.Schedule(methodCall, atDate);
    }
}
