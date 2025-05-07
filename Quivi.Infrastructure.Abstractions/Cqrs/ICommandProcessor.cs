namespace Quivi.Infrastructure.Abstractions.Cqrs
{
    public interface ICommandProcessor : IDisposable
    {
        TResult Execute<TResult>(ICommand<TResult> command);
        void Execute(ICommand command);
    }
}
