using Microsoft.AspNetCore.SignalR;
using Quivi.SignalR.Extensions;

namespace Quivi.SignalR.Hubs.Guests
{
    public class GuestsHub : Hub<IGuestClient>
    {
        public Task JoinMerchantEvents(string merchantId) => this.WithMerchantId(merchantId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveMerchantEvents(string channelId) => this.WithMerchantId(channelId, async group =>
        {
            await group.RemoveAsync(Context.ConnectionId);
        });

        public Task JoinChannelEvents(string channelId) => this.WithChannelId(channelId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveChannelEvents(string channelId) => this.WithChannelId(channelId, async group =>
        {
            await group.RemoveAsync(Context.ConnectionId);
        });

        public Task JoinChannelProfileEvents(string channelProfileId) => this.WithChannelProfileId(channelProfileId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveChannelProfileEvents(string channelProfileId) => this.WithChannelProfileId(channelProfileId, async group =>
        {
            await group.RemoveAsync(Context.ConnectionId);
        });

        public Task JoinJobEvents(string jobId) => this.WithJobId(jobId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveJobEvents(string jobId) => this.WithJobId(jobId, async group =>
        {
            await group.RemoveAsync(Context.ConnectionId);
        });

        public Task JoinTransactionEvents(string transactionId) => this.WithTransactionId(transactionId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveTransactionEvents(string transactionId) => this.WithTransactionId(transactionId, async group =>
        {
            await group.RemoveAsync(Context.ConnectionId);
        });
    }
}