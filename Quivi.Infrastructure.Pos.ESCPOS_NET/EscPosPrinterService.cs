using Quivi.Infrastructure.Abstractions.Pos.EscPos;

namespace Quivi.Infrastructure.Pos.ESCPOS_NET
{
    public class EscPosPrinterService : IEscPosPrinterService
    {
        const int FeedLineStart = 0;
        const int FeedLineEnd = 5;

        public string Get(TestPrinterParameters request)
        {
            using(var printer = new CharacterSafeCommandEmitter())
            {
                var now = DateTime.Now;
                return printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize()
                        )
                    .ConcatIf(!request.PingOnly, () => new[]{
                        printer.PrintLine(""),
                        printer.AlignToSides(now.ToString("yyyy-MM-dd"), now.ToString("HH:mm:ss")),
                        printer.PrintLine(""),
                        printer.PrintLine(""),
                        printer.CenterAlign(),
                        printer.PrintLine(request.Title),
                        printer.PrintLine(request.Message),
                        printer.FeedLines(FeedLineEnd),
                        printer.FullCut()
                    })
                    .Encode();
            }
        }
    }
}
