using MassTransit;
using Quivi.Printer.Contracts;
using Quivi.Printer.Service.Helpers;
using System.Net;
using System.Net.Sockets;

namespace Quivi.Printer.Service.Consumers
{
    public class PrintMessageConsumer : IConsumer<PrintMessage>
    {
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly ILogger<PrintMessageConsumer> logger;

        public PrintMessageConsumer(ISendEndpointProvider sendEndpointProvider,
                                    ILogger<PrintMessageConsumer> logger)
        {
            this.sendEndpointProvider = sendEndpointProvider;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PrintMessage> context)
        {
            var printerMessage = context.Message;
            foreach (var target in printerMessage.Targets)
            {
                await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Started);

                var data = Convert.FromBase64String(printerMessage.Content);
                var address = target.Address?.Trim().ToLower();
                if (string.IsNullOrEmpty(address))
                {
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Failed, $"Invalid address: {address}");
                    return;
                }

                if (address.StartsWith("usb:"))
                {
                    await SendViaUsb(printerMessage, target, data, address);
                    return;
                }

                await SendViaIp(printerMessage, target, data, address);
            }
        }

        private async Task SendViaIp(PrintMessage printerMessage, PrintMessageTarget target, byte[] data, string address)
        {
            try
            {
                UriBuilder uriBuilder = new UriBuilder(address);
                if (uriBuilder.Port == 80)
                    uriBuilder.Port = 9100;

                IPAddress ipAddress = IPAddress.Parse(uriBuilder.Host);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, uriBuilder.Port);

                using (TcpClient client = new TcpClient())
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    await client.ConnectAsync(endPoint, cancellationTokenSource.Token);
                    var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    client.Close();
                }

                await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Success, string.Empty);
            }
            catch (OperationCanceledException e)
            {
                //5 seconds connection timeout reached
                await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Unreachable, $"{e.Message} {e.StackTrace}");
            }
            catch (SocketException e)
            {
                if (new[] { SocketError.ConnectionRefused, SocketError.NotConnected }.Contains(e.SocketErrorCode))
                    //connection refused
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Unreachable, $"{e.Message} {e.StackTrace}");
                else
                    //other issues
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Failed, $"{e.Message} {e.StackTrace}");
            }
            catch (Exception e)
            {
                await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Failed, $"{e.Message} {e.StackTrace}");
            }
        }

        private async Task SendViaUsb(PrintMessage printerMessage, PrintMessageTarget target, byte[] data, string address)
        {
            //communicate via usb
            try
            {
                var printerName = address.Split("usb:", StringSplitOptions.RemoveEmptyEntries).First();

                var status = RawPrinterHelper.GetPrinterStatus(printerName);

                if (!status.IsOnline && (!printerMessage.EnqueueIfOffline || !status.IsAvailableOffline))
                {
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Unreachable, $"Printer is offline and EnqueueIfOffline={printerMessage.EnqueueIfOffline} and IsAvailableOffline={status.IsAvailableOffline}. [{status.Log}]");
                    return;
                }

                var result = RawPrinterHelper.SendBytesToPrinter(printerName, data, out string error);
                if (result)
                {
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Success, status.Log);
                }
                else
                    await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Unreachable, error);

            }
            catch (Exception e)
            {
                await StatusUpdate(printerMessage.MerchantId, printerMessage.Id, target.Id, PrintStatus.Failed, $"{e.Message} {e.StackTrace}");
            }
        }

        private async Task StatusUpdate(string merchantId, string messageId, string printerId, PrintStatus status, string? message = null)
        {
            switch (status)
            {
                case PrintStatus.Started: logger.Log(LogLevel.Information, $"Message {messageId} via Printer {printerId} was received!"); break;
                case PrintStatus.Success: logger.Log(LogLevel.Information, $"Message {messageId} via Printer {printerId} was printed successfully!"); break;
                case PrintStatus.Unreachable: logger.Log(LogLevel.Warning, $"Message {messageId} could not be printed by Printer {printerId} since it was Unreachable!"); break;
                case PrintStatus.Failed:
                    logger.Log(LogLevel.Error, $"Message {messageId} could not be printed by Printer {printerId} due to an error!");
                    if (message != null)
                        logger.Log(LogLevel.Error, message);
                    break;
                default:
                    break;
            }

            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:status"));
            await endpoint.Send(new PrintMessageStatusUpdate
            {
                MerchantId = merchantId,
                MessageId = messageId,
                PrinterId = printerId,
                Status = status,
                UtcDate = DateTime.UtcNow,
            });
        }
    }
}
