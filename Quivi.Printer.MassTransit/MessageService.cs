using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quivi.Printer.Contracts;

namespace Quivi.Printer.MassTransit
{
    public class MessageService : IHostedService, IConsumer<PrintMessageStatusUpdate>
    {
        private readonly IBusControl busControl;
        private readonly IServiceProvider serviceProvider;

        public MessageService(IBusControl busControl, IServiceProvider serviceProvider)
        {
            this.busControl = busControl;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken) => busControl.StartAsync();
        public Task StopAsync(CancellationToken cancellationToken) => busControl.StopAsync();

        public async Task Consume(ConsumeContext<PrintMessageStatusUpdate> context)
        {
            await using(var scope = serviceProvider.CreateAsyncScope())
            {
                var messageConnector = scope.ServiceProvider.GetRequiredService<PrinterMessageConnector>();
                await messageConnector.Consume(context);
            }
        }
    }
}
