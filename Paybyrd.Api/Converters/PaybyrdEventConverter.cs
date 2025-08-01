using Paybyrd.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Paybyrd.Api.Converters
{
    public class PaybyrdEventConverter : JsonConverter<PaybyrdEvent>
    {
        public override PaybyrdEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            // Read the Event first
            string? eventType = GetProperty<string>(nameof(PaybyrdEvent.Event), root, options);

            object? content = null;
            string contentPropertyName = options.PropertyNamingPolicy?.ConvertName(nameof(PaybyrdEvent.Content)) ?? nameof(PaybyrdEvent.Content);
            if (root.TryGetProperty(contentPropertyName, out var contentProp))
            {
                switch (eventType)
                {
                    case EventType.Transaction.Payment.Success:
                    case EventType.Transaction.Payment.Canceled:
                    case EventType.Transaction.Payment.Error:
                    case EventType.Transaction.Payment.Failed:
                    case EventType.Transaction.Payment.Pending:
                        content = JsonSerializer.Deserialize<Models.Payment>(contentProp.GetRawText(), options);
                        break;
                    default:
                        break;
                }
            }

            var basicEvent = new PaybyrdEvent
            {
                AttemptId = GetProperty<string>(nameof(PaybyrdEvent.AttemptId), root, options) ?? throw new Exception(),
                Id = GetProperty<string>(nameof(PaybyrdEvent.Id), root, options) ?? throw new Exception(),
                PaymentMethod = GetProperty<string>(nameof(PaybyrdEvent.PaymentMethod), root, options) ?? throw new Exception(),
                SettingsId = GetProperty<string>(nameof(PaybyrdEvent.SettingsId), root, options) ?? throw new Exception(),
                CreatedAt = GetProperty<DateTime>(nameof(PaybyrdEvent.CreatedAt), root, options),
                Event = eventType ?? throw new Exception(),
                Content = content,
            };

            return basicEvent;
        }

        public override void Write(Utf8JsonWriter writer, PaybyrdEvent value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(PaybyrdEvent.Event)) ?? nameof(PaybyrdEvent.Event));
            JsonSerializer.Serialize(writer, value.Event, options);

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(PaybyrdEvent.Content)) ?? nameof(PaybyrdEvent.Content));
            JsonSerializer.Serialize(writer, value.Content, options);

            writer.WriteEndObject();
        }

        private T? GetProperty<T>(string propertyName, JsonElement root, JsonSerializerOptions options)
        {
            string eventPropertyName = options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            if (root.TryGetProperty(eventPropertyName, out var eventProp))
                return JsonSerializer.Deserialize<T>(eventProp.GetRawText(), options);
            return default(T?);
        }
    }
}
