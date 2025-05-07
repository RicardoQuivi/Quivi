using Quivi.Domain.Entities;

namespace Quivi.Application.Commands
{
    public interface IUpdatableTranslations<T> : IUpdatableRelationship<T, Language>
    {
    }
}
