namespace Quivi.Backoffice.Api.Responses.Channels
{
    public class GenerateChannelQrCodesResponse : AResponse
    {
        public required string Base64Content { get; init; }
    }
}