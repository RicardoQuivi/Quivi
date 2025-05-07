using System.Linq.Expressions;

namespace Quivi.Infrastructure.Extensions
{
    public static class ExpressionExtensions
    {
        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly Expression _oldParameter;
            private readonly Expression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, Expression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _oldParameter)
                {
                    return _newParameter;
                }
                return base.VisitParameter(node);
            }
        }

        public static Expression<Func<T, T2>> Combine<T, T2>(this Expression<Func<T, T2>> expr1, Expression<Func<T, T2>> expr2, Expression<Func<T2, T2, T2>> builder)
        {
            var parameter = expr1.Parameters[0];

            var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
            var expr2Transformed = visitor.Visit(expr2.Body);

            var visitor1 = new ReplaceParameterVisitor(builder.Parameters[0], expr1.Body);
            var result = visitor1.Visit(builder.Body);
            var visitor2 = new ReplaceParameterVisitor(builder.Parameters[1], expr2Transformed);
            result = visitor2.Visit(result);

            return Expression.Lambda<Func<T, T2>>(result, parameter);
        }
    }
}
