namespace Quivi.Infrastructure.Abstractions.Cqrs
{
    public interface IQueryProcessor : IDisposable
    {
        TResult Execute<TResult>(IQuery<TResult> query);
        void Execute(IQuery query);
    }
}