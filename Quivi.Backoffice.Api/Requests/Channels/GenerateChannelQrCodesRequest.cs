namespace Quivi.Backoffice.Api.Requests.Channels
{
    public class GenerateChannelQrCodesRequest : ARequest
    {
        public IEnumerable<string>? ChannelIds { get; init; }
        public string? MainText { get; init; }
        public string? SecondaryText { get; init; }
    }
}