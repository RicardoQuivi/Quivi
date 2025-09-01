namespace Quivi.Infrastructure.Abstractions.Pos.EscPos
{
    public class EndOfDayClosingParameters
    {
        public required string EmployeeDesignation { get; init; }
        public required string PrinterDescription { get; init; }

        public required string Title { get; init; }
        public required string ActionDateTime { get; init; }
        public required string LocalDesignation { get; init; }
        public required IList<PaymentMethod> PaymentMethods { get; init; }

        public required string Total { get; init; }
        public required string Amount { get; init; }
        public required string Tips { get; init; }

        public required string TotalLabel { get; init; }
        public required string AmountLabel { get; init; }
        public required string TipsLabel { get; init; }
    }

    public class PaymentMethod
    {
        public required string Name { get; init; }
        public required string Total { get; init; }
        public required string Amount { get; init; }
        public required string Tips { get; init; }
    }
}