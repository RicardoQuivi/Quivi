using Quivi.Infrastructure.Abstractions.Cqrs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Quivi.Infrastructure.Cqrs
{
    public class CommandProcessor : ICommandProcessor
    {
        private ConcurrentDictionary<Type, Func<IServiceProvider, object>> CommandHandlerFactoryDictionary { get; } = new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();

        private readonly IServiceProvider serviceProvider;

        public CommandProcessor(IServiceProvider container) => serviceProvider = container;

        [DebuggerStepThrough]
        public void Execute(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using (var handler = GetCommandHandler(command))
            {
                handler.Handle(command);
            }
        }

        private IDisposableCommandHandler<ICommand> GetCommandHandler(ICommand command)
        {
            Func<IServiceProvider, object> factory = CommandHandlerFactoryDictionary.GetOrAdd(command.GetType(), (type) =>
            {
                Type handlerProxyType = typeof(CommandHandlerProxy<>).MakeGenericType(type);
                ConstructorInfo? constructorInfo = handlerProxyType.GetConstructor(new[] { typeof(IServiceProvider) });
                if (constructorInfo == null)
                    throw new Exception("This should never happen");

                ParameterExpression containerArgument = Expression.Parameter(typeof(IServiceProvider));
                Expression constructor = Expression.New(constructorInfo, containerArgument);
                return Expression.Lambda<Func<IServiceProvider, IDisposableCommandHandler<ICommand>>>(constructor, containerArgument).Compile();
            });
            return (IDisposableCommandHandler<ICommand>)factory(serviceProvider);
        }

        #region CommandHandlerProxy for void
        private interface IDisposableCommandHandler<in TCommand> : IDisposable, ICommandHandler<TCommand> where TCommand : ICommand
        {
        }

        private class CommandHandlerProxy<TCommandImplementation> : IDisposableCommandHandler<ICommand> where TCommandImplementation : class, ICommand
        {
            ICommandHandler<TCommandImplementation> Instance { get; }

            [DebuggerStepThrough]
            public CommandHandlerProxy(IServiceProvider c)
            {
                object? rawService = c.GetService(typeof(ICommandHandler<TCommandImplementation>));
                if (rawService == null)
                    throw new Exception("This should never happen");

                ICommandHandler<TCommandImplementation> service = (ICommandHandler<TCommandImplementation>)rawService;
                Instance = service;
            }

            public void Handle(ICommand command) => Instance.Handle((TCommandImplementation)command);

            public void Dispose() => (Instance as IDisposable)?.Dispose();
        }
        #endregion


        [DebuggerStepThrough]
        public TResult Execute<TResult>(ICommand<TResult> command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using (var handler = GetCommandHandler(command))
            {
                var result = handler.Handle(command);
                return result;
            }
        }

        private IDisposableCommandHandler<ICommand<TResult>, TResult> GetCommandHandler<TResult>(ICommand<TResult> command)
        {
            Func<IServiceProvider, object> factory = CommandHandlerFactoryDictionary.GetOrAdd(command.GetType(), (type) =>
            {
                Type handlerProxyType = typeof(CommandHandlerProxy<,>).MakeGenericType(type, typeof(TResult));
                ConstructorInfo? constructorInfo = handlerProxyType.GetConstructor(new[] { typeof(IServiceProvider) });
                if (constructorInfo == null)
                    throw new Exception("This should never happen");

                ParameterExpression containerArgument = Expression.Parameter(typeof(IServiceProvider));
                Expression constructor = Expression.New(constructorInfo, containerArgument);
                return Expression.Lambda<Func<IServiceProvider, IDisposableCommandHandler<ICommand<TResult>, TResult>>>(constructor, containerArgument).Compile();
            });
            return (IDisposableCommandHandler<ICommand<TResult>, TResult>)factory(serviceProvider);
        }

        #region CommandHandlerProxy for TResult
        private interface IDisposableCommandHandler<in TCommand, TResult> : IDisposable, ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
        {
        }

        private class CommandHandlerProxy<TCommandImplementation, TResult> : IDisposableCommandHandler<ICommand<TResult>, TResult> where TCommandImplementation : class, ICommand<TResult>
        {
            ICommandHandler<TCommandImplementation, TResult> Instance { get; }

            [DebuggerStepThrough]
            public CommandHandlerProxy(IServiceProvider c)
            {
                object? rawService = c.GetService(typeof(ICommandHandler<TCommandImplementation, TResult>));
                if (rawService == null)
                    throw new Exception("This should never happen");

                ICommandHandler<TCommandImplementation, TResult> service = (ICommandHandler<TCommandImplementation, TResult>)rawService;
                Instance = service;
            }

            public TResult Handle(ICommand<TResult> command) => Instance.Handle((TCommandImplementation)command);

            public void Dispose() => (Instance as IDisposable)?.Dispose();
        }
        #endregion

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
