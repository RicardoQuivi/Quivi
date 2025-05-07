namespace Quivi.Domain.Entities.Pos
{
    [Flags]
    public enum EmployeeRestrictions
    {
        None = 0,
        RemoveItems = 1 << 0,
        ApplyDiscounts = 1 << 2,
        OnlyOwnTransactions = 1 << 3,
        OnlyTransactionsOfLast24Hours = 1 << 4,
        TransferingItems = 1 << 5,
        SessionsAccess = 1 << 6,
        Refunds = 1 << 7,
    }
}
