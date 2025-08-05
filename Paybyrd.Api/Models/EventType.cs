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

        public static class Order
        {
            public const string Created = "order.created";
            public const string Pending = "order.pending";
            public const string Paid = "order.paid";
            public const string Refunded = "order.refunded";
            public const string Canceled = "order.canceled";
            public const string Expired = "order.expired";
            public const string TemporaryFailed = "order.temporaryfailed";
        }
    }
}