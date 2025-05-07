namespace Quivi.Application.Commands
{
    public interface IUpdatableEntity
    {
        bool HasChanges { get; }
    }
}
