namespace FacturaLusa.v2.Exceptions
{
    public class FacturaLusaException : Exception
    {
        public string ErrorMessage { get; }
        public ErrorType ErrorType { get; }

        public FacturaLusaException(string description, ErrorType errorType) : base(description)
        {
            ErrorMessage = description;
            ErrorType = errorType;
        }
    }
}