using Microsoft.AspNetCore.SignalR;
using Quivi.SignalR.Extensions;

namespace Quivi.SignalR.Hubs.Guests
{
    public class GuestsHub : Hub<IGuestClient>
    {
        public Task JoinChannelEvents(string channelId) => this.WithChannelId(channelId, async group =>
        {
            await group.AddAsync(Context.ConnectionId);
        });

        public Task LeaveChannelEvents(string channelId) => this.WithChannelId(channelId, async group =>
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
    }
}