namespace Quivi.Infrastructure.Apis
{
    public readonly struct Optional<T>
    {
        private readonly bool _isSet;
        private readonly T? _value;

        public Optional(T? value)
        {
            _value = value;
            _isSet = true;
        }

        public bool IsSet => _isSet;
        public T? Value => _value;

        public static implicit operator Optional<T>(T? value) => new Optional<T>(value);
        public static implicit operator T?(Optional<T> b) => b.Value;
    }
}
