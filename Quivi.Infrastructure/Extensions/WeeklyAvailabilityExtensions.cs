using Quivi.Domain.Entities;
using Quivi.Domain.Repositories.EntityFramework.Functions;
using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Extensions
{
    public static class WeeklyAvailabilityExtensions
    {
        private static readonly Lazy<MethodInfo> ToTimeZoneMethod = new Lazy<MethodInfo>(() => typeof(QuiviDbFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                                                                                        .Where(s => s.Name == nameof(QuiviDbFunctions.ToTimeZone))
                                                                                                                        .Where(s => s.GetParameters().First().ParameterType == typeof(string))
                                                                                                                        .Single());
        private static readonly Lazy<MethodInfo> ToWeeklyAvailabilitySecondsMethod = new Lazy<MethodInfo>(() => typeof(QuiviDbFunctions).GetMethod(nameof(QuiviDbFunctions.ToWeeklyAvailabilityInSeconds), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)!);
        private static readonly Lazy<MethodInfo> GenericAnyMethod = new Lazy<MethodInfo>(() => typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                                                                                .Where(s => s.Name == nameof(Enumerable.Any))
                                                                                                                .Where(s => s.GetParameters().Length == 2)
                                                                                                                .Single());

        /// <summary>
        /// Returns a TimeSpan of the difference between the provided time and the previous Sunday at 00:00:00.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>The difference between the provided time and the previous Sunday at 00:00:00.</returns>
        public static TimeSpan ToWeeklyAvailability(this DateTime time)
        {
            var aux = time.AddDays(-1 * (int)time.DayOfWeek).Date;
            return time - aux;
        }

        public static bool IsAvailable<T>(this IEnumerable<T> availabilities, DateTime merchantDateTime) where T : IWeeklyAvailability, new()
        {
            var aux = ToWeeklyAvailability(merchantDateTime);
            return availabilities.Where(day => day.StartAt <= aux && aux <= day.EndAt).Any();
        }

        public static IQueryable<TModel> WithWeeklyAvailabilities<TModel, T>(this IQueryable<TModel> queryable,
                                                                    Expression<Func<TModel, IEnumerable<T>>> calendarPropertyExpression,
                                                                    Expression<Func<TModel, string?>> timezoneExpression,
                                                                    DateTime availableTime,
                                                                    Expression<Func<TModel, bool>>? overrideExpression = null)
                                                                            where T : IDomainWeeklyAvailability
        {
            if (new[] { ExpressionType.MemberAccess, ExpressionType.Parameter }.Contains(calendarPropertyExpression.Body.NodeType) == false)
                throw new ArgumentException("Invalid " + nameof(calendarPropertyExpression));

            if (timezoneExpression.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Invalid " + nameof(timezoneExpression));

            if (overrideExpression != null && overrideExpression.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Invalid " + nameof(timezoneExpression));

            ConstantExpression availableTimeConstant = Expression.Constant(availableTime.ToString("yyyy-MM-dd HH:mm:ss"), typeof(string));

            var parameter = Expression.Parameter(typeof(TModel), "model");

            //Convert both input expressions to have input the parameter instead of whatever they have
            var calendarDaysExpr = ReplaceParameter(calendarPropertyExpression.Body, parameter);
            var timeZoneExpr = ReplaceParameter(timezoneExpression.Body, parameter);

            //QuiviDbFunctions.ToTimeZone(availableTime, timezoneExpression) | Note: the result of this operation will be called {availableTimeTZ} on the next comments
            var availableTimeTZExpr = Expression.Call(ToTimeZoneMethod.Value, availableTimeConstant, timeZoneExpr);

            //QuiviDbFunctions.ToWeeklyAvailabilityInSeconds(availableTimeTZ) | Note: the result of this operation will be called {weeklyAvailabilityInSeconds} on the next comments
            var weeklyAvailabilityInSeconds = Expression.Call(ToWeeklyAvailabilitySecondsMethod.Value, availableTimeTZExpr);

            //m.WeeklyAvailabilities.Any({validateDaysExpression})
            MethodInfo anyMethod = GenericAnyMethod.Value.MakeGenericMethod(typeof(T));
            Expression<Func<T, bool>> validateDaysExpr = GetValidateCalendarExpression<T>(weeklyAvailabilityInSeconds);
            var anyExpr = Expression.Call(anyMethod, calendarDaysExpr, validateDaysExpr);
            if (overrideExpression == null)
                return queryable.Where(Expression.Lambda<Func<TModel, bool>>(anyExpr, parameter));

            var overrideExpr = ReplaceParameter((overrideExpression.Body as MemberExpression)!, parameter);
            return queryable.Where(Expression.Lambda<Func<TModel, bool>>(Expression.Or(overrideExpr, anyExpr), parameter));
        }

        #region Private Helper Functions
        private static MemberExpression ReplaceParameter(MemberExpression memberExpression, ParameterExpression parameter)
        {
            Expression target;
            if (memberExpression.Expression is ParameterExpression)
                target = parameter;
            else
                target = ReplaceParameter(memberExpression.Expression, parameter);

            return Expression.MakeMemberAccess(target, memberExpression.Member);
        }

        private static MethodCallExpression ReplaceParameter(MethodCallExpression methodCall, ParameterExpression parameter)
        {
            // Replace the instance on which the method is called (may be null for static methods)
            Expression? instance = methodCall.Object != null ? ReplaceParameter(methodCall.Object, parameter) : null;

            // Replace arguments recursively (if needed)
            var arguments = methodCall.Arguments.Select(arg => ReplaceParameter(arg, parameter)).ToArray();
            return Expression.Call(instance, methodCall.Method, arguments);
        }

        private static LambdaExpression ReplaceParameter(LambdaExpression lambda, ParameterExpression newParameter)
        {
            // We assume there's a single parameter in the original lambda for simplicity
            var originalParameter = lambda.Parameters[0];

            // Replace body with new parameter
            var replacedBody = new ParameterReplacer(originalParameter, newParameter).Visit(lambda.Body);

            return Expression.Lambda(replacedBody, newParameter);
        }

        private static Expression ReplaceParameter(Expression? expression, ParameterExpression parameter)
        {
            if (expression is MemberExpression memberExpression)
                return ReplaceParameter(memberExpression, parameter);

            if (expression is MethodCallExpression methodCallExpression)
                return ReplaceParameter(methodCallExpression, parameter);

            if (expression is LambdaExpression lambdaExpression)
                return ReplaceParameter(lambdaExpression, parameter);

            if (expression is ParameterExpression)
                return parameter;

            throw new NotSupportedException($"Expression type {expression?.GetType().Name} is not supported.");
        }

        private static BinaryExpression GetValidateCalendarExpression<T>(Expression availableTimeTZExpr, Expression parameter) where T : IDomainWeeklyAvailability
        {
            //Equivalent to
            //return parameter.StartAtSeconds <= availableTimeTZExpr && availableTimeTZExpr <= parameter.EndAtSeconds;

            PropertyInfo startAtSecondsProperty = typeof(T).GetProperty(nameof(IDomainWeeklyAvailability.StartAtSeconds))!;
            var startAt = Expression.Property(parameter, startAtSecondsProperty);
            var isGreaterOrEqualExpr = Expression.LessThanOrEqual(startAt, availableTimeTZExpr);

            PropertyInfo endAtSecondsProperty = typeof(T).GetProperty(nameof(IDomainWeeklyAvailability.EndAtSeconds))!;
            var endAt = Expression.Property(parameter, endAtSecondsProperty);
            var isLessOrEqualExpr = Expression.LessThanOrEqual(availableTimeTZExpr, endAt);

            return Expression.And(isGreaterOrEqualExpr, isLessOrEqualExpr);
        }

        private static Expression<Func<T, bool>> GetValidateCalendarExpression<T>(Expression availableTimeTZExpr) where T : IDomainWeeklyAvailability
        {
            //Equivalent to
            //return (T t) => t.StartAtSeconds <= availableTimeTZExpr && availableTimeTZExpr <= t.EndAtSeconds;

            var parameter = Expression.Parameter(typeof(T), "weeklyAvailability");
            var result = GetValidateCalendarExpression<T>(availableTimeTZExpr, parameter);
            Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(result, parameter);
            return expression;
        }
        #endregion

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression oldParameter;
            private readonly ParameterExpression newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                this.oldParameter = oldParameter;
                this.newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == oldParameter ? newParameter : base.VisitParameter(node);
            }
        }
    }
}