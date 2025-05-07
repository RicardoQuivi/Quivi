namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public class MailAttachment
    {
        public required string Name { get; set; }
        public required byte[] Bytes { get; set; }
    }
}
