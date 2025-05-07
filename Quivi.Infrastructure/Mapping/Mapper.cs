using Quivi.Infrastructure.Abstractions.Mapping;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Mapping
{
    public class Mapper : IMapper
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>> FromFactoryDictionary { get; } = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>>();
        private IServiceProvider ServiceProvider { get; }

        public Mapper(IServiceProvider container) => ServiceProvider = container;

        public TResult Map<TResult>(object? model)
        {
            if (model == default)
                return default;

            var factory = GetMapFunction<TResult>(model.GetType());
            return factory(ServiceProvider, model);
        }

        public IEnumerable<TResult> Map<TResult>(IEnumerable model) => MapItems<TResult>(model);

        private IEnumerable<TResult> MapItems<TResult>(IEnumerable model)
        {
            var type = model.GetType();
            Type? genericType = null;
            if (type.IsArray)
                genericType = type.GetElementType();
            else
            {
                var enumerableInterfaces = type.GetInterfaces()
                                                .Where(t => t.IsGenericType)
                                                .Where(t => t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                                .Select(t => t.GetGenericArguments()[0]);
                genericType = enumerableInterfaces.FirstOrDefault() ?? type.GetGenericArguments()[0];
            }

            if (genericType == null)
                throw new Exception("This should never happen");

            var factory = GetMapFunction<TResult>(genericType);
            foreach (var t in model)
                yield return factory(ServiceProvider, t);
        }

        private Func<IServiceProvider, object, TResult> GetMapFunction<TResult>(Type modelType)
        {
            var toDictionary = FromFactoryDictionary.GetOrAdd(modelType, (type) => new ConcurrentDictionary<Type, object>());
            object factory = toDictionary.GetOrAdd(typeof(TResult), GenerateMapperGetter<TResult>(modelType));

            return (Func<IServiceProvider, object, TResult>)factory;
        }

        [DebuggerStepThrough]
        private Func<Type, object> GenerateMapperGetter<TResult>(Type modelType)
        {
            return (type) =>
            {
                Type tFromType = modelType;
                Type handlerProxyType = typeof(IMapperHandler<,>).MakeGenericType(tFromType, type);

                Exception? originalException = null;
                //The following loop purpose is to check if we have any Handler capable of mapping
                //the provided model Type or any of it's base classes.
                //This is particularly useful due to EntityFramework DynamicProxies.
                while (true)
                {
                    try
                    {
                        var result = ServiceProvider.GetService(handlerProxyType);
                        if (result == null)
                            throw new NullReferenceException();

                        break;
                    }
                    catch (NullReferenceException e)
                    {
                        if (originalException == null)
                            originalException = e;

                        if (tFromType.BaseType == null || tFromType.BaseType == typeof(System.Enum))
                            throw originalException;

                        tFromType = tFromType.BaseType;
                        handlerProxyType = typeof(IMapperHandler<,>).MakeGenericType(tFromType, type);
                    }
                }

                //The following block code returns a function equivalent to:
                //(IServiceProvider p, object model) => {
                //    var rawService = p.GetService(typeof(IMapperHandler<TFrom, TResult>));
                //    var service = (IMapperHandler<TFrom, TResult>)rawService;
                //    return service.Map((TFrom)model);
                //}

                ParameterExpression containerArgument = Expression.Parameter(typeof(IServiceProvider));
                ParameterExpression modelArgument = Expression.Parameter(typeof(object));

                MethodInfo? getServiceMethodInfo = typeof(IServiceProvider).GetMethod(nameof(ServiceProvider.GetService), new Type[] { typeof(Type) });
                if (getServiceMethodInfo == null)
                    throw new Exception("This should never happen");

                Expression typeArgument = Expression.Constant(handlerProxyType);
                Expression getServiceInstanceCall = Expression.Call(containerArgument, getServiceMethodInfo, typeArgument);
                UnaryExpression castedServiceInstance = Expression.Convert(getServiceInstanceCall, handlerProxyType);
                UnaryExpression castedModelArgument = Expression.Convert(modelArgument, tFromType);
                MethodInfo? mapMethodInfo = handlerProxyType.GetMethod(nameof(IMapperHandler<object, TResult>.Map)); //Note: It's irrelevant the types we are passing, since we only want the name of the method
                if(mapMethodInfo == null)
                    throw new Exception("This should never happen");

                Expression mapCall = Expression.Call(castedServiceInstance, mapMethodInfo, castedModelArgument);
                return Expression.Lambda<Func<IServiceProvider, object, TResult>>(mapCall, containerArgument, modelArgument).Compile();
            };
        }

        public TResult? Map<TFrom, TResult>(TFrom? model)
        {
            if (model == null)
                return default;

            var rawHandler = ServiceProvider.GetService(typeof(IMapperHandler<TFrom, TResult>));
            if (rawHandler == null)
                throw new Exception($"Implementation of IMapperHandler<{typeof(TFrom).Name},{typeof(TResult).Name}> not found.");

            var handler = (IMapperHandler<TFrom, TResult>)rawHandler;
            return handler.Map(model);
        }

        public IEnumerable<TResult> Map<TFrom, TResult>(IEnumerable<TFrom> model)
        {
            var rawHandler = ServiceProvider.GetService(typeof(IMapperHandler<TFrom, TResult>));
            if (rawHandler == null)
                throw new Exception($"Implementation of IMapperHandler<{typeof(TFrom).Name},{typeof(TResult).Name}> not found.");

            var handler = (IMapperHandler<TFrom, TResult>)rawHandler;
            return model.Select(handler.Map);
        }
    }
}
