namespace Quivi.Pos.Api.Dtos
{
    public class Printer
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public bool PrintConsumerInvoice { get; set; }
        public bool PrintPreparationRequest { get; set; }
        public bool PrintConsumerBill { get; set; }
        public bool CanOpenCashDrawer { get; set; }
        public bool CanPrintCloseDayTotals { get; set; }
    }
}