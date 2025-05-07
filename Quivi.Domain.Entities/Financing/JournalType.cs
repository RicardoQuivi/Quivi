namespace Quivi.Domain.Entities.Financing
{
    //TODO: Most likely we don't need this many values
    public enum JournalType
    {
        Deposit = 0,
        Withdrawal = 1,
        Capture = 2,
        Refund = 3,
        Authorization = 4,
        Consent = 5,
        Surcharge = 6,
        TransactionFees = 7,
        AccountingClosing = 8,
        ConsumerBalanceSettlement = 100,
        MerchantBilling = 1210,
        MerchantBillingVat = 1211,
        MerchantReimbursement = 1220,
    }
}
