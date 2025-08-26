using Quivi.Domain.Entities;
using Quivi.Infrastructure.Abstractions.Events.Data;
using System.Collections;

namespace Quivi.Application.Commands
{
    public class ChangedEntity<TEntityRelationship>
    {
        public required TEntityRelationship Entity { get; init; }
        public required EntityOperation Operation { get; init; }
    }

    public class UpdatableRelationshipEntity<TEntityRelationship, TUpdatable, TKey> : IUpdatableRelationship<TUpdatable, TKey>
                                                                                                            where TEntityRelationship : IEntity
                                                                                                            where TUpdatable : IUpdatableEntity
                                                                                                            where TKey : notnull
    {
        private readonly ICollection<TEntityRelationship> relationships;
        private readonly HashSet<TEntityRelationship> originalRelationships;
        private readonly Dictionary<TKey, (TEntityRelationship Model, TUpdatable Updatable)> relationshipsDictionary;
        private readonly Func<TEntityRelationship, TUpdatable> converter;
        private readonly Func<TKey, TEntityRelationship> constructor;

        public UpdatableRelationshipEntity(ICollection<TEntityRelationship> relationships,
                                                Func<TEntityRelationship, TKey> getKey,
                                                Func<TEntityRelationship, TUpdatable> converter,
                                                Func<TKey, TEntityRelationship> constructor)
        {
            this.relationships = relationships;
            this.converter = converter;
            this.constructor = constructor;
            relationshipsDictionary = relationships.ToDictionary(getKey, t => (t, converter(t)));
            originalRelationships = relationships.ToHashSet();
        }

        public TUpdatable this[TKey key] => relationshipsDictionary[key].Updatable;

        public bool ContainsKey(TKey key) => relationshipsDictionary.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (relationshipsDictionary.TryGetValue(key, out var exists))
                relationships.Remove(exists.Model);
            return relationshipsDictionary.Remove(key);
        }

        public void Clear()
        {
            relationshipsDictionary.Clear();
            relationships.Clear();
        }

        public void Upsert(TKey key, Action<TUpdatable> t)
        {
            if (relationshipsDictionary.TryGetValue(key, out var exists))
            {
                t(exists.Updatable);
                return;
            }

            var newEntry = constructor(key);
            var newLanguage = (newEntry, converter(newEntry));
            relationships.Add(newLanguage.newEntry);
            relationshipsDictionary[key] = newLanguage;
            t(newLanguage.Item2);
        }

        public IEnumerator<TUpdatable> GetEnumerator() => relationshipsDictionary.Values.Select(s => s.Updatable).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public virtual bool HasChanges => ChangedEntities.Any();

        public IEnumerable<ChangedEntity<TEntityRelationship>> ChangedEntities
        {
            get
            {
                var existentRelationships = new HashSet<TEntityRelationship>();
                foreach (var e in relationshipsDictionary.Values)
                {
                    existentRelationships.Add(e.Model);

                    EntityOperation? state = null;
                    if (e.Updatable.HasChanges)
                        state = EntityOperation.Update;
                    else if (originalRelationships.Contains(e.Model) == false)
                        state = EntityOperation.Create;

                    if (state.HasValue)
                        yield return new ChangedEntity<TEntityRelationship>
                        {
                            Entity = e.Model,
                            Operation = state.Value,
                        };
                }

                foreach (var e in originalRelationships.Where(e => existentRelationships.Contains(e) == false))
                    yield return new ChangedEntity<TEntityRelationship>
                    {
                        Entity = e,
                        Operation = EntityOperation.Delete,
                    };
            }
        }
    }
}