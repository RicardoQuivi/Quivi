namespace Quivi.Printer.Contracts
{
    public enum MessageContentType
    {
        /// <summary>
        /// No content is sent, 
        /// the device should simply ping the printer
        /// </summary>
        Empty = 0,
        /// <summary>
        /// A raw EscPos is sent to printer
        /// </summary>
        EscPos = 1,
        /// <summary>
        /// A document is received and sent to printer
        /// </summary>
        Url = 2,
    }
}
