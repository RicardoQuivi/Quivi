namespace Quivi.Application.Configurations
{
    public interface IPaybyrdSettings
    {
        string Host { get; }
        string WebHooksHost { get; }
        string ApiKey { get; }
    }

    public class PaybyrdSettings : IPaybyrdSettings
    {
        public required string Host { get; init; }
        public required string WebHooksHost { get; init; }
        public required string ApiKey { get; init; }
    }
}