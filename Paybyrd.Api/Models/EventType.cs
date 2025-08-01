namespace Paybyrd.Api.Models
{
    public static class EventType
    {
        public static class Transaction
        {
            public static class Payment
            {
                public const string Pending = "transaction.payment.pending";
                public const string Success = "transaction.payment.success";
                public const string Failed = "transaction.payment.failed";
                public const string Error = "transaction.payment.error";
                public const string Canceled = "transaction.payment.canceled";
            }
        }
    }
}