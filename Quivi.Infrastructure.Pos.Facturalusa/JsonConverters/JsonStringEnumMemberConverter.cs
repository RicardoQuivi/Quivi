using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Quivi.Infrastructure.Pos.Facturalusa.JsonConverters
{
    internal class JsonStringEnumMemberConverter<T> : JsonConverter where T : Enum
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var enumString = reader.Value?.ToString();
            var matchingValue = Enum.GetValues(objectType).Cast<T>().FirstOrDefault(value => IsMatch(value, enumString));

            if (matchingValue == null)
                throw new JsonException($"Unable to convert \"{enumString}\" to enum \"{objectType}\".");

            return matchingValue;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(GetEnumMemberValue((T?)value));
        }

        private bool IsMatch(T value, string? targetExpression)
        {
            if(targetExpression == null)
                return false;

            string? valueStr = GetEnumMemberValue(value);
            if (valueStr == null)
                return false;

            var regex = new Regex(valueStr);
            return regex.IsMatch(targetExpression);
        }

        private string? GetEnumMemberValue(T? value)
        {
            return value?.GetType()
               .GetMember(value.ToString())
               .First()
               .GetCustomAttribute<JsonPropertyAttribute>()
               ?.PropertyName;
        }
    }
}