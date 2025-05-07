namespace Quivi.Backoffice.Api.Dtos
{
    public class PrinterWorker
    {
        public required string Id { get; init; }
        public required string Identifier { get; init; }
        public string? Name { get; init; }
    }
}