namespace FacturaLusa.v2.Dtos.Requests.Sales
{
    public class DownloadSaleFileRequest
    {
        public required long SaleId { get; init; }
        public string? Language { get; init; }
        public DocumentFormat? Format { get; init; }
        public int? PaperSize { get; init; }
        public int? PaperLeftMargin { get; init; }
        public int? PaperRightMargin { get; init; }
        public int? PaperTopMargin { get; init; }
        public int? PaperBottomMargin { get; init; }
        public DocumentIssue Issue { get; init; }
        public int? MaxItemsPerPage { get; init; }
    }
}