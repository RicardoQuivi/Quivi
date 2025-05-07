namespace Quivi.Pos.Api.Dtos
{
    public enum EmployeeRestriction
    {
        RemoveItems,
        ApplyDiscounts,
        OnlyOwnTransactions,
        OnlyTransactionsOfLast24Hours,
        TransferingItems,
        SessionsAccess,
        Refunds,
    }

    public class Employee
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public bool HasPinCode { get; init; }
        public TimeSpan? InactivityLogoutTimeout { get; init; }
        public required IEnumerable<EmployeeRestriction> Restrictions { get; init; }
    }
}
