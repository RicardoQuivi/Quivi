using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.JsonConverters
{
    internal class JsonDateOnlyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return new[] { typeof(DateTime), typeof(DateTime?) }.Contains(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var valueStr = reader.Value?.ToString();
            if(valueStr == null)
                return null;

            var date = DateTime.Parse(valueStr);
            date = DateTime.SpecifyKind(date, DateTimeKind.Local);
            return date;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if(value == null)
                writer.WriteValue(value);
            else
                writer.WriteValue(((DateTime)value).ToString("yyyy-MM-dd"));
        }
    }
}