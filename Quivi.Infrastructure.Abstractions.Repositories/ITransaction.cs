namespace Quivi.Infrastructure.Abstractions.Repositories
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
