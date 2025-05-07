using System.ComponentModel.DataAnnotations;

namespace Quivi.Infrastructure.Exceptions
{
    public class ValidationStateResult : ValidationResult
    {
        public object? PropertyValue { get; }
        public string? ErrorCode { get; }
        public Type PropertyType { get; }
        public object? Context { get; }

        public ValidationStateResult(string propertyName, object? propertyValue, string message, string code, object context, Type errorType) : this(propertyName, propertyValue, message, code, errorType)
        {
            Context = context;
        }


        public ValidationStateResult(string propertyName, object? propertyValue, string message, string code, Type errorType) : this(propertyName, propertyValue, message, errorType)
        {
            ErrorCode = code;
        }

        public ValidationStateResult(string propertyName, object? propertyValue, string message, Type errorType) : base(message, new[] { propertyName })
        {
            PropertyValue = propertyValue;
            PropertyType = errorType;
        }
    }

    public abstract class InvalidModelStateExceptionBase : Exception
    {
        public object? Model { get; }
        public abstract IEnumerable<ValidationStateResult> ValidationResults { get; }
        public bool HasErrors => ValidationResults.Any();

        public InvalidModelStateExceptionBase(object? model) : base()
        {
            Model = model;
        }
    }
}
