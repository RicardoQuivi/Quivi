using Quivi.Infrastructure.Abstractions.Cqrs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Cqrs
{
    public class QueryProcessor : IQueryProcessor
    {
        private ConcurrentDictionary<Type, Func<IServiceProvider, object>> QueryHandlerFactoryDictionary { get; } = new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();
        private readonly IServiceProvider serviceProvider;

        public QueryProcessor(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        [DebuggerStepThrough]
        public void Execute(IQuery query)
        {
            using (var handler = GetQueryHandler(query))
            {
                handler.Handle(query);
            }
        }

        [DebuggerStepThrough]
        private IDisposableQueryHandler<IQuery> GetQueryHandler(IQuery query)
        {
            Func<IServiceProvider, object> factory = QueryHandlerFactoryDictionary.GetOrAdd(query.GetType(), (type) =>
            {
                Type handlerProxyType = typeof(QueryHandlerProxy<>).MakeGenericType(type);
                ConstructorInfo? constructorInfo = handlerProxyType.GetConstructor(new[] { typeof(IServiceProvider) });
                if (constructorInfo == null)
                    throw new Exception("This should never happen");

                ParameterExpression containerArgument = Expression.Parameter(typeof(IServiceProvider));
                Expression constructor = Expression.New(constructorInfo, containerArgument);
                return Expression.Lambda<Func<IServiceProvider, IDisposableQueryHandler<IQuery>>>(constructor, containerArgument).Compile();
            });
            return (IDisposableQueryHandler<IQuery>)factory(serviceProvider);
        }

        private interface IDisposableQueryHandler<in TQuery> : IDisposable, IQueryHandler<TQuery> where TQuery : IQuery
        {
        }

        private class QueryHandlerProxy<TQueryImplementation> : IDisposableQueryHandler<IQuery> where TQueryImplementation : class, IQuery
        {
            IQueryHandler<TQueryImplementation> Instance { get; }

            [DebuggerStepThrough]
            public QueryHandlerProxy(IServiceProvider c)
            {
                object? rawService = c.GetService(typeof(IQueryHandler<TQueryImplementation>));
                if (rawService == null)
                    throw new Exception("This should never happen");

                IQueryHandler<TQueryImplementation> service = (IQueryHandler<TQueryImplementation>)rawService;
                Instance = service;
            }

            [DebuggerStepThrough]
            public void Handle(IQuery query) => Instance.Handle((TQueryImplementation)query);

            [DebuggerStepThrough]
            public void Dispose() => (Instance as IDisposable)?.Dispose();
        }

        [DebuggerStepThrough]
        public TResult Execute<TResult>(IQuery<TResult> query)
        {
            using (var handler = GetQueryHandler(query))
            {
                var result = handler.Handle(query);
                return result;
            }
        }

        [DebuggerStepThrough]
        private IDisposableQueryHandler<IQuery<TResult>, TResult> GetQueryHandler<TResult>(IQuery<TResult> query)
        {
            Func<IServiceProvider, object> factory = QueryHandlerFactoryDictionary.GetOrAdd(query.GetType(), (type) =>
            {
                Type handlerProxyType = typeof(QueryHandlerProxy<,>).MakeGenericType(type, typeof(TResult));
                ConstructorInfo? constructorInfo = handlerProxyType.GetConstructor(new[] { typeof(IServiceProvider) });
                if (constructorInfo == null)
                    throw new Exception("This should never happen");

                ParameterExpression containerArgument = Expression.Parameter(typeof(IServiceProvider));
                Expression constructor = Expression.New(constructorInfo, containerArgument);
                return Expression.Lambda<Func<IServiceProvider, IDisposableQueryHandler<IQuery<TResult>, TResult>>>(constructor, containerArgument).Compile();
            });
            return (IDisposableQueryHandler<IQuery<TResult>, TResult>)factory(serviceProvider);
        }

        private interface IDisposableQueryHandler<in TQuery, TResult> : IDisposable, IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
        {
        }

        private class QueryHandlerProxy<TQueryImplementation, TResult> : IDisposableQueryHandler<IQuery<TResult>, TResult> where TQueryImplementation : class, IQuery<TResult>
        {
            IQueryHandler<TQueryImplementation, TResult> Instance { get; }

            [DebuggerStepThrough]
            public QueryHandlerProxy(IServiceProvider c)
            {
                object? rawService = c.GetService(typeof(IQueryHandler<TQueryImplementation, TResult>));
                if(rawService == null)
                    throw new Exception("This should never happen");

                IQueryHandler<TQueryImplementation, TResult> service = (IQueryHandler<TQueryImplementation, TResult>)rawService;
                Instance = service;
            }

            [DebuggerStepThrough]
            public TResult Handle(IQuery<TResult> query) => Instance.Handle((TQueryImplementation)query);

            [DebuggerStepThrough]
            public void Dispose() => (Instance as IDisposable)?.Dispose();
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}