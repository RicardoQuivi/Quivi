using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Quivi.Infrastructure.Jobs.Hangfire.Activators
{
    public class ServiceProviderJobActivator : JobActivator
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ServiceProviderJobActivator(IServiceProvider container)
        {
            _scopeFactory = container.GetRequiredService<IServiceScopeFactory>();
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ScopedJobActivator(_scopeFactory.CreateAsyncScope());
        }

        public override object ActivateJob(Type jobType)
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            return scope.ServiceProvider.GetService(jobType)!;
        }

        private class ScopedJobActivator : JobActivatorScope
        {
            private readonly AsyncServiceScope _scope;

            public ScopedJobActivator(AsyncServiceScope scope)
            {
                _scope = scope;
            }

            public override object? Resolve(Type type)
            {
                return _scope.ServiceProvider.GetService(type);
            }

            public override void DisposeScope()
            {
                _scope.DisposeAsync().GetAwaiter().GetResult();
            }
        }
    }
}