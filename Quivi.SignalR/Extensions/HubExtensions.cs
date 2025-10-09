using Microsoft.AspNetCore.SignalR;

namespace Quivi.SignalR.Extensions
{
    public interface IGroup<T> where T : class
    {
        Task AddAsync(string connectionId);
        Task RemoveAsync(string connectionId);
        T Client { get; }
    }

    public static class HubExtensions
    {
        private abstract class AGroup<T> : IGroup<T> where T : class
        {
            private readonly IGroupManager groupManager;
            private readonly IHubClients<T> hubClients;
            private readonly string groupName;

            public AGroup(IGroupManager groupManager, IHubClients<T> hubClients, string id, string groupPrefix)
            {
                this.groupManager = groupManager;
                this.hubClients = hubClients;
                this.groupName = $"{groupPrefix}/{id}";
            }

            public Task AddAsync(string connectionId) => groupManager.AddToGroupAsync(connectionId, groupName);
            public Task RemoveAsync(string connectionId) => groupManager.RemoveFromGroupAsync(connectionId, groupName);
            public T Client => hubClients.Group(groupName);
        }

        private class UserGroup<T> : AGroup<T> where T : class
        {
            public UserGroup(IGroupManager groupManager, IHubClients<T> hubClients, string userId) : base(groupManager, hubClients, userId, "Users")
            {
            }
        }

        private class MerchantGroup<T> : AGroup<T> where T : class
        {
            public MerchantGroup(IGroupManager groupManager, IHubClients<T> hubClients, string merchantId) : base(groupManager, hubClients, merchantId, "Users")
            {
            }
        }

        private class ChannelGroup<T> : AGroup<T> where T : class
        {
            public ChannelGroup(IGroupManager groupManager, IHubClients<T> hubClients, string channelId) : base(groupManager, hubClients, channelId, "Channels")
            {
            }
        }

        private class ChannelProfileGroup<T> : AGroup<T> where T : class
        {
            public ChannelProfileGroup(IGroupManager groupManager, IHubClients<T> hubClients, string channelId) : base(groupManager, hubClients, channelId, "ChannelProfiles")
            {
            }
        }

        private class JobGroup<T> : AGroup<T> where T : class
        {
            public JobGroup(IGroupManager groupManager, IHubClients<T> hubClients, string jobId) : base(groupManager, hubClients, jobId, "Jobs")
            {
            }
        }

        private class TransactionGroup<T> : AGroup<T> where T : class
        {
            public TransactionGroup(IGroupManager groupManager, IHubClients<T> hubClients, string transactionId) : base(groupManager, hubClients, transactionId, "Transactions")
            {
            }
        }

        public static async Task WithUserId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new UserGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithUserId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new UserGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }

        public static async Task WithMerchantId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new MerchantGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithMerchantId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new MerchantGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }

        public static async Task WithChannelProfileId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new ChannelProfileGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithChannelProfileId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new ChannelProfileGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }

        public static async Task WithChannelId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new ChannelGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithChannelId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new ChannelGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }

        public static async Task WithJobId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new JobGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithJobId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new JobGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }

        public static async Task WithTransactionId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new TransactionGroup<T>(context.Groups, context.Clients, id);
            await func(group);
        }

        public static async Task WithTransactionId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var group = new TransactionGroup<T>(hub.Groups, hub.Clients, id);
            await func(group);
        }
    }
}