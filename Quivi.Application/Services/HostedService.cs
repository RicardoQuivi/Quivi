using Microsoft.Extensions.Hosting;
using Quivi.Infrastructure.Abstractions.Services;

namespace Quivi.Application.Services
{
    public class HostedService<T> : IHostedService where T : IStartUpService
    {
        private readonly T service;

        public HostedService(T service)
        {
            this.service = service;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = this.service.StartUpAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
