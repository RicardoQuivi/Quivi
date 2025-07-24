using Mjml.Net;
using Quivi.Infrastructure.Abstractions.Services.Mailing;
using System.Collections.Concurrent;
using System.Reflection;

namespace Quivi.Infrastructure.Mailing.EmailEngine.Mjml
{
    public class MjmlEmailEngine : IEmailEngine, IFileLoader
    {
        private static ConcurrentDictionary<string, string> LoadedTemplates = new ConcurrentDictionary<string, string>();

        public string ConfirmEmail(ConfirmEmailParameters parameters) => GenerateHtml(nameof(ConfirmEmail), parameters);
        public string ForgotPassword(ForgotPasswordParameters parameters) => GenerateHtml(nameof(ForgotPassword), parameters);
        public string PurchaseConfirmation(PurchaseConfirmationParameters parameters) => GenerateHtml(nameof(PurchaseConfirmation), new
        {
            parameters.MerchantName,
            Date = parameters.Date.ToString("dd/MM/yyyy HH:mm"),
            Amount = parameters.Amount.ToString("0.00", new System.Globalization.CultureInfo("pt-PT")) + " €",
            parameters.TransactionId,
        });
        public string OrderInvoice(OrderInvoiceParameters parameters) => GenerateHtml(nameof(OrderInvoice), new
        {
            parameters.MerchantName,
            parameters.InvoiceName,
            Date = parameters.Date.ToString("dd/MM/yyyy HH:mm"),
            Amount = parameters.Amount.ToString("0.00", new System.Globalization.CultureInfo("pt-PT")) + " €",
            parameters.TransactionId,
        });

        public string SurchargeInvoice(SurchargeInvoiceParameters parameters) => GenerateHtml(nameof(SurchargeInvoice), new
        {
            parameters.MerchantName,
            parameters.InvoiceName,
            Date = parameters.Date.ToString("dd/MM/yyyy HH:mm"),
            Amount = parameters.Amount.ToString("0.00", new System.Globalization.CultureInfo("pt-PT")) + " €",
            parameters.TransactionId,
        });

        #region Helpers
        private string GenerateHtml<T>(string templateName, T parameters)
        {
            var template = GetTemplate(templateName);

            foreach(var property in typeof(T).GetProperties())
            {
                var value = property.GetMethod!.Invoke(parameters, []);
                template = template.Replace($"{{{{{property.Name}}}}}", value?.ToString());
            }
            return template;
        }

        private string GetTemplate(string templateName) => LoadedTemplates.GetOrAdd(templateName, t =>
        {
            var template = ReadFile($"{templateName}.mjml");

            var mjmlRenderer = new MjmlRenderer();
            var options = new MjmlOptions
            {
                Beautify = false,
                FileLoader = () => this,
            };

            var (html, errors) = mjmlRenderer.Render(template, options);
            if (errors?.Any() == true)
                throw new Exception("Problem parsing template");

            return html;
        });

        private static string ReadFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{typeof(MjmlEmailEngine).Namespace}.Templates.{file}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream!))
            {
                var template = reader.ReadToEnd();
                return template;
            }
        }

        public string? LoadText(string path)
        {
            if (path.StartsWith("./"))
                return ReadFile(path.Replace("./", ""));
            throw new NotImplementedException();
        }
        #endregion
    }
}