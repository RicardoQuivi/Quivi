using ESCPOS_NET.Emitters;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;

namespace Quivi.Infrastructure.Pos.ESCPOS_NET
{
    public class EscPosPrinterService : IEscPosPrinterService
    {
        const int FeedLineStart = 0;
        const int FeedLineEnd = 5;

        public string Get(TestPrinterParameters request)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                return printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize()
                    ).ConcatIf(!request.PingOnly, () => [
                        printer.PrintLine(""),
                        printer.AlignToSides(request.Timestamp.ToString("yyyy-MM-dd"), request.Timestamp.ToString("HH:mm:ss")),
                        printer.PrintLine(""),
                        printer.PrintLine(""),
                        printer.CenterAlign(),
                        printer.PrintLine(request.Title),
                        printer.PrintLine(request.Message),
                        printer.FeedLines(FeedLineEnd),
                        printer.FullCut()
                    ]).Encode();
            }
        }

        public string Get(PreparationRequestParameters request)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                var result = printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize(),
                        printer.FeedLines(FeedLineStart),
                        printer.CenterAlign(),
                        printer.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold),
                        printer.PrintLine(request.Title),
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("")
                    )
                    .ConcatIf(!string.IsNullOrEmpty(request.OrderPlaceholder), () => [
                        printer.PrintLine(""),
                        printer.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold),
                        printer.PrintLine(request.OrderPlaceholder!),
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("")
                    ])
                    .ConcatWith(
                        printer.LeftAlign(),
                        printer.Print(request.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                        printer.PrintLine(""),
                        printer.AlignToSides(request.SessionPlaceholder, request.ChannelPlaceholder),
                        printer.PrintLine(""),
                        printer.CenterAlign(),
                        printer.PrintLine("------------------------------------------------"),
                        printer.LeftAlign()
                    )
                    .ConcatWith(
                        printer.SetStyles(PrintStyle.DoubleHeight)
                    )
                    .ConcatWithForeach(request.Items, item =>
                        printer
                            .StartConcat(
                                printer.AlignToSides(item.Name, item.Quantity == 0 ? string.Empty : $" {(item.Add == false ? "-" : "x")} {item.Quantity}"),
                                printer.PrintLine("")
                            )
                            .ConcatWithForeach(FlatenizeModifiers(item.Modifiers), modifier =>
                                printer
                                    .StartConcat(
                                        printer.LeftAlign(),
                                        printer.PrintLine($"    {modifier}")
                                    )
                            )
                    );

                if (request.AdditionalInfo?.Any() == true)
                {
                    result = result.ConcatWith(
                        printer.CenterAlign(),
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("------------------------------------------------")
                    );
                    foreach (var item in request.AdditionalInfo)
                    {
                        result = result.ConcatIf(
                            !string.IsNullOrEmpty(item.Value), () => new[]
                            {
                                printer.LeftAlign(),
                                printer.SetStyles(PrintStyle.Bold),
                                printer.Print($"{item.Key}: "),
                                printer.SetStyles(PrintStyle.None),
                                printer.Print(item.Value),
                                printer.PrintLine(""),
                            }
                        );
                    }
                }

                result = result.ConcatWith(
                    printer.SetStyles(PrintStyle.None),
                    printer.CenterAlign(),
                    printer.PrintLine("------------------------------------------------"),
                    printer.FeedLines(FeedLineEnd),
                    printer.FullCut()
                );

                return result.Encode();
            }
        }

        public string Get(NewPendingOrderParameters request)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                var result = printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize(),
                        printer.FeedLines(FeedLineStart),
                        printer.CenterAlign(),
                        printer.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold),
                        printer.PrintLine(request.Title),
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("")
                    )
                    .ConcatIf(!string.IsNullOrEmpty(request.OrderPlaceholder), () =>
                    [
                        printer.PrintLine(""),
                        printer.SetStyles(PrintStyle.DoubleHeight | PrintStyle.DoubleWidth | PrintStyle.Bold),
                        printer.PrintLine(request.OrderPlaceholder)
                    ])
                    .ConcatWith(
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("")
                    )
                    .ConcatWith(
                        printer.LeftAlign(),
                        printer.Print(request.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                        printer.PrintLine(""),
                        printer.AlignToSides(string.Empty, request.ChannelPlaceholder),
                        printer.PrintLine("")
                    );

                result = result.ConcatWith(
                        printer.SetStyles(PrintStyle.None),
                        printer.CenterAlign(),
                        printer.PrintLine("------------------------------------------------"),
                        printer.FeedLines(FeedLineEnd),
                        printer.FullCut()
                    );

                return result.Encode();
            }
        }

        public string Get(OpenCashDrawerParameters request)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                return printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize(),
                        printer.CashDrawerOpenPin2(),
                        printer.CashDrawerOpenPin5()
                    )
                    .Encode();
            }
        }

        public string Get(AppendToDocumentParameters request)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                var result = printer.StartConcat(
                                printer.Clear(),
                                printer.Initialize(),
                                request.EscPosContent
                            ).ConcatWith(
                                printer.SetStyles(PrintStyle.None),
                                printer.CenterAlign(),
                                printer.PrintLine(""),
                                printer.PrintLine("")
                            );

                foreach (var item in request.AdditionalInfo ?? Enumerable.Empty<string>())
                {
                    result = result.ConcatIf(!string.IsNullOrEmpty(item), () =>
                    [
                        printer.Print(item),
                        printer.PrintLine(""),
                        printer.PrintLine(""),
                    ]);
                }

                result = result.ConcatWith(
                    printer.PrintLine(""),
                    printer.PrintLine(request.ChannelName),
                    printer.FeedLines(FeedLineEnd),
                    printer.FullCut()
                );

                return result.Encode();
            }
        }

        public string Get(EndOfDayClosingParameters parameters)
        {
            using (var printer = new CharacterSafeCommandEmitter())
            {
                var result = printer
                    .StartConcat(
                        printer.Clear(),
                        printer.Initialize(),
                        printer.FeedLines(FeedLineStart),
                        printer.CenterAlign(),
                        printer.SetStyles(PrintStyle.DoubleHeight),
                        printer.PrintLine(parameters.Title),
                        printer.SetStyles(PrintStyle.None),
                        printer.PrintLine("")
                    )
                    .ConcatIf(
                        !string.IsNullOrEmpty(parameters.PrinterDescription), () => new[]
                        {
                            printer.PrintLine(parameters.PrinterDescription),
                            printer.PrintLine("")
                        })
                    .ConcatWith(
                        printer.LeftAlign(),
                        printer.Print(parameters.ActionDateTime),
                        printer.PrintLine(""),
                        printer.PrintLine(parameters.EmployeeDesignation),
                        printer.PrintLine(parameters.LocalDesignation),
                        printer.PrintLine(""),
                        printer.CenterAlign(),
                        printer.PrintLine("------------------------------------------------"),
                        printer.LeftAlign()
                    )
                    .ConcatWithForeach(parameters.PaymentMethods, item =>
                        printer.StartConcat(
                            printer.SetStyles(PrintStyle.DoubleHeight),
                            printer.AlignToSides(item.Name, item.Total),
                            printer.PrintLine("")
                        ).ConcatWith(
                            printer.LeftAlign(),
                            printer.SetStyles(PrintStyle.None),
                            printer.AlignToSides($"  {parameters.AmountLabel}", item.Amount),
                            printer.AlignToSides($"  {parameters.TipsLabel}", item.Tips)
                        )
                    );

                result = result.ConcatWith(
                    printer.SetStyles(PrintStyle.None),
                    printer.CenterAlign(),
                    printer.PrintLine("------------------------------------------------"),
                    printer.SetStyles(PrintStyle.DoubleHeight),
                    printer.AlignToSides(parameters.TotalLabel, parameters.Total),
                    printer.LeftAlign(),
                    printer.SetStyles(PrintStyle.None),
                    printer.AlignToSides($"  {parameters.AmountLabel}", parameters.Amount),
                    printer.AlignToSides($"  {parameters.TipsLabel}", parameters.Tips),
                    printer.FeedLines(FeedLineEnd),
                    printer.FullCut()
                );

                return result.Encode();
            }
        }

        private IEnumerable<string> FlatenizeModifiers(IEnumerable<BasePreparationRequestItem>? modifiers)
        {
            if (modifiers == null)
                return Enumerable.Empty<string>();

            return modifiers
                .GroupBy(x => x.Name)
                .SelectMany(g => Enumerable.Repeat(g.Key, g.Sum(m => m.Quantity)))
                .ToList();
        }
    }
}