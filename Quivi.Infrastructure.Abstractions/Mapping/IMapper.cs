using System.Collections;

namespace Quivi.Infrastructure.Abstractions.Mapping
{
    public interface IMapper
    {
        TResult Map<TResult>(object? model);
        IEnumerable<TResult> Map<TResult>(IEnumerable model);
        TResult? Map<TFrom, TResult>(TFrom? model);
        IEnumerable<TResult> Map<TFrom, TResult>(IEnumerable<TFrom> model);
    }
}
