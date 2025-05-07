using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Jobs.Hangfire.Filters
{
    public class ConditionalAutomaticRetryAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
    {
        private readonly AutomaticRetryAttribute _automaticRetryAttribute = new AutomaticRetryAttribute();

        public int Attempts
        {
            get => _automaticRetryAttribute.Attempts;
            set => _automaticRetryAttribute.Attempts = value;
        }

        public int[] DelaysInSeconds
        {
            get => _automaticRetryAttribute.DelaysInSeconds;
            set => _automaticRetryAttribute.DelaysInSeconds = value;
        }

        public Func<long, int> DelayInSecondsByAttemptFunc
        {
            get => _automaticRetryAttribute.DelayInSecondsByAttemptFunc;
            set => _automaticRetryAttribute.DelayInSecondsByAttemptFunc = value;
        }

        public AttemptsExceededAction OnAttemptsExceeded
        {
            get => _automaticRetryAttribute.OnAttemptsExceeded;
            set => _automaticRetryAttribute.OnAttemptsExceeded = value;
        }

        public bool LogEvents
        {
            get => _automaticRetryAttribute.LogEvents;
            set => _automaticRetryAttribute.LogEvents = value;
        }

        /// <summary>
        /// Name of the Method to run on fail. The method should be public, static, have a FailedState argument and return a bool, stating if
        /// it should fail immediately in case of true, or should retry in case of false.
        /// </summary>
        public string? ShouldFailMethodName { get; }
        public string? OnFailureMethodName { get; set; }
        public string? OnRetryMethodName { get; set; }

        public ConditionalAutomaticRetryAttribute(string conditionFuncName) : base()
        {
            ShouldFailMethodName = conditionFuncName;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _automaticRetryAttribute.OnStateApplied(context, transaction);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                if (OnRetryMethodName != null)
                    RunRetryMethod(context);

                if (!string.IsNullOrWhiteSpace(ShouldFailMethodName))
                {
                    var jobType = context.BackgroundJob.Job.Type;
                    var method = jobType.GetMethod(ShouldFailMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    bool? shouldFail = method == null ? null : method.Invoke(null, new object[] { failedState }) as bool?;
                    if (shouldFail == true)
                    {
                        //Workaround to avoid retries
                        context.SetJobParameter("RetryCount", 1000);

                        if (OnFailureMethodName != null)
                            RunFailMethod(context);
                    }
                }
            }

            _automaticRetryAttribute.OnStateElection(context);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            _automaticRetryAttribute.OnStateUnapplied(context, transaction);
        }

        private void RunFailMethod(ElectStateContext context)
        {
            var job = context.BackgroundJob.Job;

            var failMethod = job.Type.GetMethods().SingleOrDefault(m => m.Name == OnFailureMethodName);
            if (failMethod == null)
                throw new Exception($"Could not found method {OnFailureMethodName} on for job using type {job.Type.FullName}");

            ParameterExpression parameter = Expression.Parameter(job.Type);
            var zip = job.Args.Zip(failMethod.GetParameters(), (a, b) => new
            {
                Value = a,
                Type = b.ParameterType,
            });
            Expression expression = Expression.Call(parameter, failMethod, zip.Select(a => Expression.Constant(a.Value, a.Type)));

            var funcType = typeof(Func<,>).MakeGenericType(job.Type, failMethod.ReturnType);
            var lambdaType = typeof(Expression<>).MakeGenericType(funcType);
            var lambdaExpression = Expression.Lambda(expression, parameter);

            var enqueueJobMethod = GetType().GetMethod(nameof(EnqueueJobTask), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(job.Type);
            enqueueJobMethod.Invoke(this, [lambdaExpression]);
        }

        private void RunRetryMethod(ElectStateContext context)
        {
            int attemptNumber = context.GetJobParameter<int>("RetryCount") + 1;
            var job = context.BackgroundJob.Job;

            var retryMethod = job.Type.GetMethods().SingleOrDefault(m => m.Name == OnRetryMethodName);
            if (retryMethod == null)
                throw new Exception($"Could not found method {OnRetryMethodName} on for job using type {job.Type.FullName}");

            ParameterExpression parameter = Expression.Parameter(job.Type);
            Expression expression = Expression.Call(parameter, retryMethod, new object[] { Attempts, attemptNumber }.Concat(job.Args).Select(a => Expression.Constant(a)));

            var funcType = typeof(Func<,>).MakeGenericType(job.Type, retryMethod.ReturnType);
            var lambdaType = typeof(Expression<>).MakeGenericType(funcType);
            var lambdaExpression = Expression.Lambda(expression, parameter);

            var enqueueJobMethod = GetType().GetMethod(nameof(EnqueueJobTask), BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(job.Type);
            enqueueJobMethod.Invoke(this, new object[] { lambdaExpression });
        }

        private void EnqueueJobTask<T>(Expression<Func<T, Task>> expression) => BackgroundJob.Enqueue(expression);
    }
}
