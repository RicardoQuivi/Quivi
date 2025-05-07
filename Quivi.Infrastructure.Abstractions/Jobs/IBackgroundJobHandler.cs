using System.Linq.Expressions;

namespace Quivi.Infrastructure.Abstractions.Jobs
{
    public enum JobContinuation
    {
        OnFinished,
        OnSuccessfullyFinished,
    }

    public interface IBackgroundJobHandler
    {
        /// <summary>
        /// Creates a new fire-and-forget job based on a given method call expression.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Enqueue(Expression<Action> methodCall);
        /// <summary>
        /// Creates a new fire-and-forget job based on a given method call expression.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Enqueue(Expression<Func<Task>> methodCall);
        /// <summary>
        /// Creates a new fire-and-forget job based on a given method call expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Enqueue<T>(Expression<Action<T>> methodCall);
        /// <summary>
        /// Creates a new fire-and-forget job based on a given method call expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Enqueue<T>(Expression<Func<T, Task>> methodCall);

        /// <summary>
        /// Creates a new background job based on a specified instance method call expression and schedules it to be enqueued after a given delay.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Schedule(Expression<Action> methodCall, TimeSpan delay);
        /// <summary>
        /// Creates a new background job based on a specified instance method call expression and schedules it to be enqueued after a given delay.
        /// </summary>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
        /// <summary>
        /// Creates a new background job based on a specified instance method call expression and schedules it to be enqueued after a given delay.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
        /// <summary>
        /// Creates a new background job based on a specified instance method call expression and schedules it to be enqueued after a given delay.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        /// <param name="enqueueAt"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);


        string Schedule(Expression<Action> methodCall, DateTimeOffset atDate);
        string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset atDate);
        string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset atDate);
        string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset atDate);

        /// <summary>
        /// Creates a new background job that will wait for a successful completion of another background job to be enqueued.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string ContinueJobWith(string parentId, Expression<Action> methodCall);
        /// <summary>
        /// Creates a new background job that will wait for a successful completion of another background job to be enqueued.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string ContinueJobWith<T>(string parentId, Expression<Action<T>> methodCall);
        /// <summary>
        /// Creates a new background job that will wait for a successful completion of another background job to be enqueued.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string ContinueJobWith(string parentId, Expression<Func<Task>> methodCall);
        /// <summary>
        /// Creates a new background job that will wait for a successful completion of another background job to be enqueued.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="methodCall"></param>
        /// <param name="continuation"></param>
        /// <returns>Unique identifier of a created job.</returns>
        string ContinueJobWith(string parentId, Expression<Func<Task>> methodCall, JobContinuation continuation);
    }
}