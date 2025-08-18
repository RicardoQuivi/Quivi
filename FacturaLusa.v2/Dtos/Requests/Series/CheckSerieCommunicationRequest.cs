namespace FacturaLusa.v2.Dtos.Requests.Series
{
    public class CheckSerieCommunicationRequest
    {
        public required long Id { get; init; }
        public required DocumentType DocumentType { get; init; }
    }
}