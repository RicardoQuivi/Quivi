namespace Quivi.Infrastructure.Abstractions.Mapping
{
    public interface IMapperHandler<in TFrom, TResult>
    {
        TResult Map(TFrom model);
    }
}
