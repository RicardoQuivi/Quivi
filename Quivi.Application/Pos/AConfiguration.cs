using Newtonsoft.Json.Linq;

namespace Quivi.Application.Pos
{
    public class ParseException : Exception
    {
        public string ConnectionString { get; }
        public string Field { get; }

        public ParseException(string connectionString, string fieldName) : base($"Could not parse connection string due to misconfiguration while trying to read field '{fieldName}'")
        {
            ConnectionString = connectionString;
            Field = fieldName;
        }
    }

    public abstract class AConfiguration
    {
        public string ConnectionString { get; }

        protected IReadOnlyDictionary<string, object?> ConnectionParameters => connectionParameters.Value;

        private readonly Lazy<IReadOnlyDictionary<string, object?>> connectionParameters;

        public AConfiguration()
        {
            ConnectionString = string.Empty;
            connectionParameters = new Lazy<IReadOnlyDictionary<string, object?>>(() =>
            {
                var jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(ConnectionString) as JObject;
                return ToDictionary(jObject);
            });
        }

        public AConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
            connectionParameters = new Lazy<IReadOnlyDictionary<string, object?>>(() =>
            {
                var jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(connectionString) as JObject;
                return ToDictionary(jObject);
            });
        }

        private IReadOnlyDictionary<string, object?> ToDictionary(JObject? jObject)
        {
            Dictionary<string, object?> properties = new Dictionary<string, object?>();
            if (jObject == null)
                return properties;
            foreach (var entry in jObject)
            {
                if (entry.Value is JValue jValue)
                    properties.Add(entry.Key, jValue.Value);
                else if (entry.Value is JObject innerJObject)
                    properties.Add(entry.Key, ToDictionary(innerJObject));
                else
                    throw new NotImplementedException("Implement me");
            }
            return properties;
        }

        protected string? ExtractOptionalString(IReadOnlyDictionary<string, object?> connection, string key)
        {
            if (!connection.TryGetValue(key, out var result))
                return null;
            return result as string;
        }

        protected string ExtractString(IReadOnlyDictionary<string, object?> connection, string key) => ExtractOptionalString(connection, key) ?? throw new ParseException(ConnectionString, key);

        protected int? ExtractOptionalInt(IReadOnlyDictionary<string, object?> connection, string key)
        {
            if (!connection.TryGetValue(key, out var resultObj))
                return null;
            if (resultObj is int resultInt)
                return resultInt;
            if (resultObj is string resultStr && int.TryParse(resultStr, out var result))
                return result;
            return null;
        }

        protected int ExtractInt(IReadOnlyDictionary<string, object?> connection, string key) => ExtractOptionalInt(connection, key) ?? throw new ParseException(ConnectionString, key);

        protected bool ExtractBoolean(IReadOnlyDictionary<string, object?> connection, string key) => ExtractOptionalBoolean(connection, key) ?? throw new ParseException(ConnectionString, key);

        protected bool? ExtractOptionalBoolean(IReadOnlyDictionary<string, object?> connection, string key)
        {
            if (!connection.TryGetValue(key, out var resultObj))
                return null;

            if (resultObj is bool resultBool)
                return resultBool;

            if (resultObj is string resultStr)
            {
                if (resultStr == "1" || resultStr.ToLower() == "true")
                    return true;
                else if (resultStr == "0" || resultStr.ToLower() == "false")
                    return false;
            }
            return null;
        }

        protected TimeSpan? ExtractOptionalTimeSpan(IReadOnlyDictionary<string, object?> connection, string key)
        {
            if (!connection.TryGetValue(key, out var resultObj))
                return null;

            if (resultObj is TimeSpan resultTimeSpan)
                return resultTimeSpan;

            if (resultObj is string resultStr && TimeSpan.TryParse(resultStr, out var result))
                return result;

            return null;
        }
    }
}
