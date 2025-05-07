using Quivi.Domain.Entities;
using System.Collections;

namespace Quivi.Application.Commands
{
    public abstract class AUpdatableTranslatableEntity<T, TTranslatable, TTranslatableInterface> : IUpdatableTranslations<TTranslatableInterface>
                                                                                                            where T : ITranslation
                                                                                                            where TTranslatable : IUpdatableEntity, TTranslatableInterface
    {
        private readonly ICollection<T> _translations;
        private readonly HashSet<T> _originalTranslations;
        private readonly Dictionary<Language, (T Model, TTranslatable Translatable)> _translationsDictionary;
        private readonly Func<T, TTranslatable> _converter;
        private readonly Func<T> _constructor;

        public AUpdatableTranslatableEntity(ICollection<T> translations, Func<T, TTranslatable> converter, Func<T> constructor)
        {
            _translations = translations;
            _converter = converter;
            _constructor = constructor;
            _translationsDictionary = translations.ToDictionary(t => t.Language, t => (t, converter(t)));
            _originalTranslations = translations.ToHashSet();
        }

        public TTranslatableInterface this[Language key] => _translationsDictionary[key].Translatable;

        public bool ContainsKey(Language key) => _translationsDictionary.ContainsKey(key);

        public bool Remove(Language key)
        {
            if (_translationsDictionary.TryGetValue(key, out var exists))
                _translations.Remove(exists.Model);
            return _translationsDictionary.Remove(key);
        }

        public void Clear()
        {
            _translationsDictionary.Clear();
            _translations.Clear();
        }

        public void Upsert(Language key, Action<TTranslatableInterface> t)
        {
            if (_translationsDictionary.TryGetValue(key, out var exists))
            {
                t(exists.Translatable);
                return;
            }

            var newEntry = _constructor();
            newEntry.DeletedDate = null;
            newEntry.Language = key;
            var newLanguage = (newEntry, _converter(newEntry));
            _translations.Add(newLanguage.newEntry);
            _translationsDictionary[key] = newLanguage;
            t(newLanguage.Item2);
        }

        public IEnumerator<TTranslatableInterface> GetEnumerator() => _translationsDictionary.Values.Select(s => (TTranslatableInterface)s.Translatable).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual bool HasChanges
        {
            get
            {
                if (_translationsDictionary.Count != _originalTranslations.Count)
                    return true;

                foreach (var entry in _translationsDictionary.Values)
                {
                    if (entry.Translatable.HasChanges)
                        return true;

                    if (_originalTranslations.Contains(entry.Model) == false)
                        return true;
                }
                return false;
            }
        }

        public IUpdatableTranslations<TTranslatableInterface> Translations => this;
    }
}
