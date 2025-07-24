namespace Quivi.Application.Commands
{
    public interface IUpdatableRelationship<TEntity, TKey> : IEnumerable<TEntity>
    {
        TEntity this[TKey key] { get; }
        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        void Clear();
        void Upsert(TKey key, Action<TEntity> t);
    }
}