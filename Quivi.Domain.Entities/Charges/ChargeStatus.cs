namespace Quivi.Domain.Entities.Charges
{
    public enum ChargeStatus
    {
        Expired = -2,
        Failed = -1,

        Requested = 0,
        Processing = 1,
        Completed = 2,
    }
}