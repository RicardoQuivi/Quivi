namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public interface IEscPosPrinterService
    {
        string Get(TestPrinterParameters request);
    }
}
