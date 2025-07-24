namespace Quivi.Infrastructure.Abstractions.Services.Mailing
{
    public interface IEmailEngine
    {
        string ConfirmEmail(ConfirmEmailParameters parameters);
        string ForgotPassword(ForgotPasswordParameters parameters);
        string PurchaseConfirmation(PurchaseConfirmationParameters parameters);
        string OrderInvoice(OrderInvoiceParameters parameters);
        string SurchargeInvoice(SurchargeInvoiceParameters parameters);
    }
}