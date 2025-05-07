using Microsoft.AspNetCore.SignalR;
using Quivi.Infrastructure.Extensions;
using Quivi.SignalR.Extensions;

namespace Quivi.SignalR.Hubs.Pos
{
    public class PosHub : Hub<IPosClient>
    {
        public PosHub()
        {

        }

        public override async Task OnConnectedAsync()
        {
            await this.WithMerchantId(Context.User?.SubMerchantId(), async group =>
            {
                await group.AddAsync(Context.ConnectionId);
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await this.WithMerchantId(Context.User?.SubMerchantId(), async group =>
            {
                await group.RemoveAsync(Context.ConnectionId);
            });

            await base.OnDisconnectedAsync(exception);
        }
    }
}