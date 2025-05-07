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
        private class UserGroup<T> : IGroup<T> where T : class
        {
            private readonly IGroupManager groupManager;
            private readonly IHubClients<T> hubClients;
            private readonly string groupName;

            public UserGroup(IGroupManager groupManager, IHubClients<T> hubClients, string userId)
            {
                this.groupManager = groupManager;
                this.hubClients = hubClients;
                this.groupName = $"Users/{userId}";
            }

            public Task AddAsync(string connectionId) => groupManager.AddToGroupAsync(connectionId, groupName);
            public Task RemoveAsync(string connectionId) => groupManager.RemoveFromGroupAsync(connectionId, groupName);
            public T Client => hubClients.Group(groupName);
        }

        private class MerchantGroup<T> : IGroup<T> where T : class
        {
            private readonly IGroupManager groupManager;
            private readonly IHubClients<T> hubClients;
            private readonly string groupName;

            public MerchantGroup(IGroupManager groupManager, IHubClients<T> hubClients, string merchantId)
            {
                this.groupManager = groupManager;
                this.hubClients = hubClients;
                this.groupName = $"Merchants/{merchantId}";
            }

            public Task AddAsync(string connectionId) => groupManager.AddToGroupAsync(connectionId, groupName);
            public Task RemoveAsync(string connectionId) => groupManager.RemoveFromGroupAsync(connectionId, groupName);
            public T Client => hubClients.Group(groupName);
        }

        public static async Task WithUserId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var userGroup = new UserGroup<T>(context.Groups, context.Clients, id);
            await func(userGroup);
        }

        public static async Task WithUserId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var userGroup = new UserGroup<T>(hub.Groups, hub.Clients, id);
            await func(userGroup);
        }

        public static async Task WithMerchantId<THub, T>(this IHubContext<THub, T> context, string id, Func<IGroup<T>, Task> func) where THub : Hub<T> where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var userGroup = new MerchantGroup<T>(context.Groups, context.Clients, id);
            await func(userGroup);
        }

        public static async Task WithMerchantId<T>(this Hub<T> hub, string? id, Func<IGroup<T>, Task> func) where T : class
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var userGroup = new MerchantGroup<T>(hub.Groups, hub.Clients, id);
            await func(userGroup);
        }
    }
}
