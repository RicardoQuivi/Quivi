namespace FacturaLusa.v2.Exceptions
{
    public class NotAuthorizedException : FacturaLusaException
    {
        public NotAuthorizedException() : base("Invalid or no Api Key", ErrorType.Unauthorized)
        {
        }
    }
}
