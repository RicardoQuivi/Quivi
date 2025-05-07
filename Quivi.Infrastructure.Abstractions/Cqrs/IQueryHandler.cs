namespace Quivi.Infrastructure.Abstractions.Cqrs
{
    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }

    public interface IQueryHandler<in TQuery> where TQuery : IQuery
    {
        void Handle(TQuery query);
    }
}
