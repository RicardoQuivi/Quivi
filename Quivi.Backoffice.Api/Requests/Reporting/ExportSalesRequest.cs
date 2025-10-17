namespace Quivi.Backoffice.Api.Requests.Reporting
{
    public class ExportSalesRequest : ARequest
    {
        public class ExportSalesLabels
        {
            public required string Date { get; init; }
            public required string TransactionId { get; init; }
            public required string Invoice { get; init; }
            public required string Id { get; init; }
            public required string Method { get; init; }
            public required string MenuId { get; init; }
            public required string Item { get; init; }
            public required string UnitPrice { get; init; }
            public required string Quantity { get; init; }
            public required string Total { get; init; }
        }

        public required ExportSalesLabels Labels { get; init; }

        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}