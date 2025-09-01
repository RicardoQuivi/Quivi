namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public interface IEscPosPrinterService
    {
        string Get(TestPrinterParameters request);
        string Get(PreparationRequestParameters request);
        string Get(NewPendingOrderParameters request);
        string Get(OpenCashDrawerParameters request);
        string Get(AppendToDocumentParameters request);
        string Get(EndOfDayClosingParameters request);
    }
}