namespace ComplyPay.Dtos
{
    public class Balance
    {
        public required string Currency { get; init; }
        public int Amount { get; init; }
    }
}
