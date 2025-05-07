using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Extensions;
using Quivi.SignalR.Extensions;

namespace Quivi.SignalR.Hubs.Backoffice
{
    public class BackofficeHub : Hub<IBackofficeClient>
    {
        public BackofficeHub()
        {

        }

        public override async Task OnConnectedAsync()
        {
            await this.WithUserId(Context.User?.UserId(), async group =>
            {
                await group.AddAsync(Context.ConnectionId);
            });

            await this.WithMerchantId(Context.User?.SubMerchantId(), async group =>
            {
                await group.AddAsync(Context.ConnectionId);
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await this.WithUserId(Context.User?.UserId(), async group =>
            {
                await group.RemoveAsync(Context.ConnectionId);
            });

            await this.WithMerchantId(Context.User?.SubMerchantId(), async group =>
            {
                await group.AddAsync(Context.ConnectionId);
            });

            await base.OnDisconnectedAsync(exception);
        }
    }
}
