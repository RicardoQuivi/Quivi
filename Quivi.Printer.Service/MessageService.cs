using MassTransit;

namespace Quivi.Printer.Service
{
    public class MessageService : IHostedService
    {
        private readonly IBusControl busControl;

        public MessageService(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken) => busControl.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken) => busControl.StopAsync();
    }
}