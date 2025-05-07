using Quivi.Infrastructure.Exceptions;
using System.Linq.Expressions;

namespace Quivi.Infrastructure.Validations
{
    public class ModelStateValidator<T, Y> : IDisposable where Y : notnull
    {
        public InvalidModelStateException<T> Exception { get; }
        
        private bool disposed = false;

        public ModelStateValidator(T model)
        {
            Exception = new InvalidModelStateException<T>(model);
        }

        public InvalidModelStateException<T> AddError(Expression<Func<T, object?>> lambda, Y error)
        {
            var code = error.ToString();
            if(code is null)
                throw new ArgumentException($"{nameof(error)}.ToString() should not return null");
            string message = "";
            return Exception.WithError(lambda, message, code, error.GetType());
        }

        public InvalidModelStateException<T> AddError(Expression<Func<T, object?>> lambda, Y error, object context)
        {
            var code = error.ToString();
            if (code is null)
                throw new ArgumentException($"{nameof(error)}.ToString() should not return null");
            string message = "";
            return Exception.WithError(lambda, message, code, context, error.GetType());
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            if (Exception.HasErrors)
                throw Exception;
        }
    }
}
