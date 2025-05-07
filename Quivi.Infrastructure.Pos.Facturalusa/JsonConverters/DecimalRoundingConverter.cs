using Newtonsoft.Json;

namespace Quivi.Infrastructure.Pos.Facturalusa.JsonConverters
{
    public class DecimalRoundingConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            decimal decimalValue = (decimal)(value ?? 0.0m);
            writer.WriteValue(Math.Round(decimalValue, 2));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            decimal decimalValue = Convert.ToDecimal(reader.Value);
            return Math.Round(decimalValue, 2);
        }
    }
}