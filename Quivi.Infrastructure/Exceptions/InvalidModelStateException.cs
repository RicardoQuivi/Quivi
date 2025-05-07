using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Exceptions
{
    public class InvalidModelStateException<T> : InvalidModelStateExceptionBase
    {
        public new T? Model => (T?)base.Model;
        public override IEnumerable<ValidationStateResult> ValidationResults => _validationResults;
        protected List<ValidationStateResult> _validationResults { get; set; } = new List<ValidationStateResult>();

        public InvalidModelStateException(T model) : base(model)
        {
        }

        public InvalidModelStateException(T model, InvalidModelStateExceptionBase ex, bool copyErrors) : base(model)
        {
            if (copyErrors)
                _validationResults.AddRange(ex.ValidationResults);
        }

        public InvalidModelStateException<T> WithErrors(IEnumerable<ValidationStateResult> validationResults)
        {
            _validationResults.AddRange(validationResults);
            return this;
        }

        public InvalidModelStateException<T> WithError<Y>(Expression<Func<T, Y>> lambda, string message, string code, Type errorType)
        {
            if (lambda == null)
                throw new ArgumentNullException(nameof(lambda));
            string name = AnalyseAndGetPath(lambda.Body);
            _validationResults.Add(new ValidationStateResult(name, lambda.Compile().Invoke(Model), message, code, errorType));
            return this;
        }

        public InvalidModelStateException<T> WithError<Y>(Expression<Func<T, Y>> lambda, string message, string code, object context, Type errorType)
        {
            if (lambda == null)
                throw new ArgumentNullException(nameof(lambda));
            string name = AnalyseAndGetPath(lambda.Body);
            _validationResults.Add(new ValidationStateResult(name, lambda.Compile().Invoke(Model), message, code, context, errorType));
            return this;
        }

        private string AnalyseAndGetPath(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.ArrayIndex:
                    {
                        var binaryExpression = expression as BinaryExpression;
                        return AnalyseMemberExpressionByIndex(binaryExpression.Left as MemberExpression, binaryExpression.Right);
                    }
                case ExpressionType.Convert:
                    {
                        var operand = ((UnaryExpression)expression).Operand;
                        if (operand is MemberExpression memberExpr)
                        {
                            if (memberExpr.Expression is MemberExpression memberExpr2)
                                return $"{AnalyseAndGetPath(memberExpr2)}.{memberExpr.Member.Name}";
                            return memberExpr.Member.Name;
                        }
                        if (operand is MethodCallExpression methodCallExpression)
                            return AnalyseMethodCallExpression(methodCallExpression);
                        if (operand.NodeType == ExpressionType.ArrayIndex)
                        {
                            var binaryExpression = operand as BinaryExpression;
                            return AnalyseMemberExpressionByIndex(binaryExpression.Left as MemberExpression, binaryExpression.Right);
                        }
                        break;
                    }
                case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberExpr = expression as MemberExpression;
                        if (memberExpr.Expression is MemberExpression memberExpr2)
                            return $"{AnalyseAndGetPath(memberExpr2)}.{memberExpr.Member.Name}";
                        return memberExpr.Member.Name;
                    }
                case ExpressionType.Call: return AnalyseMethodCallExpression((MethodCallExpression)expression);
                case ExpressionType.Parameter: return "";
                case ExpressionType.Constant: return ((ConstantExpression)expression).Value as string;
            }
            throw new NotImplementedException();
        }

        private string AnalyseMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            if (method?.Name == "get_Item")
            {
                //In case it's accessing array by index
                if (method.DeclaringType.IsArray)
                    return AnalyseMemberExpressionByIndex(methodCallExpression.Object as MemberExpression, methodCallExpression.Arguments.Single());
                //In case it's accessing List by index
                if (method.DeclaringType.IsGenericType && method.DeclaringType.GetGenericTypeDefinition() == typeof(IList<>))
                    return AnalyseMemberExpressionByIndex(methodCallExpression.Object as MemberExpression, methodCallExpression.Arguments.Single());
            }
            //In case it's call to ElementAt, Treat the same way as an array
            if (method?.GetGenericMethodDefinition() == typeof(Enumerable).GetMethod(nameof(Enumerable.ElementAt)))
                return AnalyseMemberExpressionByIndex(methodCallExpression.Arguments.First() as MemberExpression, methodCallExpression.Arguments.Skip(1).Single());
            throw new NotImplementedException();
        }

        private string AnalyseMemberExpressionByIndex(MemberExpression memberExpr, Expression expressionArgument)
        {
            if (expressionArgument is ConstantExpression c)
            {
                object value = GetValue(memberExpr, c.Value);
                if (memberExpr.Expression is MemberExpression memberExpr2)
                    return $"{AnalyseAndGetPath(memberExpr2)}.{memberExpr.Member.Name}[{value}]";
                return $"{memberExpr.Member.Name}[{value}]";
            }
            else if (expressionArgument is MemberExpression m)
            {
                object value = GetValue(m);
                if (memberExpr.Expression is MemberExpression memberExpr2)
                    return $"{AnalyseAndGetPath(memberExpr2)}.{memberExpr.Member.Name}[{value}]";
                return $"{memberExpr.Member.Name}[{value}]";
            }
            throw new NotImplementedException();
        }

        private object GetValue(MemberExpression expression)
        {
            if (expression.Expression is ConstantExpression c)
                return GetValue(expression, c.Value);
            if (expression.Expression is MemberExpression m)
                return GetValue(expression, GetValue(m));
            throw new NotImplementedException();
        }

        private object GetValue(MemberExpression m, object obj)
        {
            if (obj.GetType().IsPrimitive)
                return obj;

            switch (m.Member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)m.Member).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)m.Member).GetValue(obj);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
