namespace Quivi.Domain.Entities.Financing
{
    public enum JournalMethod
    {
        PhoneNumber = 0,
        ShowQR = 1, //Mobile user shows QR Code
        ReadQR = 2, //Mobile user reads QR Code
        Integrated = 3,
        Nayax = 4,
        ContactlessCard = 5,
    }
}