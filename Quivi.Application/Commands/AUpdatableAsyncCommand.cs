using Quivi.Infrastructure.Abstractions.Cqrs;

namespace Quivi.Application.Commands
{
    public interface IUpdateCommand<T, TUpdate> : ICommand<Task<T>> where TUpdate : IUpdatableEntity
    {
        Func<TUpdate, Task> UpdateAction { get; init; }
    }

    public interface IUpdateAsyncCommand<T, TUpdate> : ICommand<Task<T>>,
                                                        IUpdateCommand<T, TUpdate>
                                                        where TUpdate : IUpdatableEntity
    {
    }

    public abstract class AUpdateAsyncCommand<T, TUpdate> : IUpdateAsyncCommand<T, TUpdate> where TUpdate : IUpdatableEntity
    {
        required public Func<TUpdate, Task> UpdateAction { get; init; }
    }
}
