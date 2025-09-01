using Quivi.Application.Commands.PrinterNotificationMessages;
using Quivi.Application.Queries.CustomChargeMethods;
using Quivi.Application.Queries.Employees;
using Quivi.Application.Queries.Locations;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Notifications;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Pos.EscPos;
using Quivi.Infrastructure.Extensions;
using System.Globalization;

namespace Quivi.Application.Commands.Pos
{
    public class GenerateEndOfDayClosingAsyncCommand : ICommand<Task>
    {
        public int MerchantId { get; init; }
        public int EmployeeId { get; init; }
        public int? LocationId { get; init; }

        public required string TitleLabel { get; init; }
        public required string PrintedByLabel { get; init; }
        public required string LocationLabel { get; init; }
        public required string AllLocationsLabel { get; init; }
        public required string TotalLabel { get; init; }
        public required string AmountLabel { get; init; }
        public required string TipsLabel { get; init; }
    }

    public class EndOfDayClosingAsyncCommandHandler : ICommandHandler<GenerateEndOfDayClosingAsyncCommand, Task>
    {
        private static readonly CultureInfo CustomCulture = GetCulture();

        private static CultureInfo GetCulture()
        {
            var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.CurrencyGroupSeparator = " "; //Use space as thousand separator
            customCulture.NumberFormat.CurrencySymbol = ""; //Remove currency symbol (optional)
            customCulture.NumberFormat.CurrencyDecimalDigits = 2; //Set 2 decimal places

            return customCulture;
        }

        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEscPosPrinterService escPosPrinterService;

        public EndOfDayClosingAsyncCommandHandler(IQueryProcessor queryProcessor,
                                                    ICommandProcessor commandProcessor,
                                                    IDateTimeProvider dateTimeProvider,
                                                    IEscPosPrinterService escPosPrinterService)
        {
            this.queryProcessor = queryProcessor;
            this.dateTimeProvider = dateTimeProvider;
            this.commandProcessor = commandProcessor;
            this.escPosPrinterService = escPosPrinterService;
        }

        public Task Handle(GenerateEndOfDayClosingAsyncCommand command) => commandProcessor.Execute(new CreatePrinterNotificationMessageAsyncCommand
        {
            MessageType = NotificationMessageType.EndOfDayClosing,
            Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetPrinterNotificationsContactsCriteria
            {
                MerchantIds = [command.MerchantId],
                MessageTypes = [NotificationMessageType.EndOfDayClosing],
                LocationIds = command.LocationId.HasValue ? [command.LocationId.Value] : null,
                IsDeleted = false,

                PageIndex = 0,
                PageSize = null,
            },
            GetContent = async () =>
            {
                var message = await GetTotalsParameters(command);
                return escPosPrinterService.Get(message);
            },
        });
        private async Task<EndOfDayClosingParameters> GetTotalsParameters(GenerateEndOfDayClosingAsyncCommand command)
        {
            var now = dateTimeProvider.GetUtcNow();
            var fromDate = now.Subtract(Infrastructure.Constants.Settlements.Offset).Date.Add(Infrastructure.Constants.Settlements.Offset);
            var toDate = fromDate.AddDays(1);

            var totalsPerPaymentMethodQuery = await queryProcessor.Execute(new GetPosChargesResumeByPaymentMethodAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                FromCapturedDate = fromDate,
                ToCapturedDate = toDate,
                IsCaptured = true,
                LocationIds = command.LocationId.HasValue ? [command.LocationId.Value] : null,
            });
            var totalsPerPaymentMethod = totalsPerPaymentMethodQuery.ToDictionary(e => e.Key.Id, e => e.Value);

            var allMerchantPaymentMethods = await queryProcessor.Execute(new GetCustomChargeMethodsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
            });

            decimal total = 0.0m;
            decimal payments = 0.0m;
            decimal tips = 0.0m;
            var resultPaymentMethods = new List<PaymentMethod>();
            foreach (var payment in allMerchantPaymentMethods.Append(new CustomChargeMethod
            {
                Id = 0,
                Name = "Quivi",
            }))
            {
                var methodTotal = 0.0m;
                var methodPayments = 0.0m;
                var methodTips = 0.0m;

                if (totalsPerPaymentMethod.TryGetValue(payment.Id, out var value))
                {
                    methodPayments = value.PaymentAmount;
                    methodTips = value.TipAmount;
                    methodTotal = methodPayments + methodTips;

                    total += methodTotal;
                    payments += methodPayments;
                    tips += methodTips;
                }

                resultPaymentMethods.Add(new PaymentMethod
                {
                    Name = payment.Name,
                    Total = FormatCurrency(methodTotal),
                    Amount = FormatCurrency(methodPayments),
                    Tips = FormatCurrency(methodTips),
                });
            }

            var employee = await GetEmployee(command);
            var merchantNow = dateTimeProvider.GetUtcNow().ToTimeZone(employee.Merchant!.TimeZone);

            return new EndOfDayClosingParameters
            {
                Title = command.TitleLabel,
                ActionDateTime = merchantNow.ToString("yyyy-MM-dd HH:mm:ss"),
                LocalDesignation = $"{command.LocationLabel}: {await GetLocalName(command) ?? command.AllLocationsLabel}",
                EmployeeDesignation = $"{command.PrintedByLabel}: {employee.Name}",
                PrinterDescription = fromDate.ToString("dd/MM/yyyy"),
                PaymentMethods = resultPaymentMethods.Select(r => new PaymentMethod
                {
                    Name = r.Name,
                    Total = r.Total,
                    Amount = r.Amount,
                    Tips = r.Tips,
                }).ToList(),
                Total = FormatCurrency(total),
                Amount = FormatCurrency(payments),
                Tips = FormatCurrency(tips),
                AmountLabel = command.AmountLabel,
                TipsLabel = command.TipsLabel,
                TotalLabel = command.TotalLabel,
            };
        }

        private async Task<string?> GetLocalName(GenerateEndOfDayClosingAsyncCommand command)
        {
            if (command.LocationId.HasValue == false)
                return null;

            var locationsQuery = await queryProcessor.Execute(new GetLocationsAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                Ids = [command.LocationId.Value],
            });
            return locationsQuery.SingleOrDefault()?.Name;
        }

        private async Task<Employee> GetEmployee(GenerateEndOfDayClosingAsyncCommand command)
        {
            var employee = await queryProcessor.Execute(new GetEmployeesAsyncQuery
            {
                MerchantIds = [command.MerchantId],
                Ids = [command.EmployeeId],
                IncludeMerchant = true,
                IsDeleted = null,
                PageSize = 1,
            });
            return employee.Single();
        }

        private static string FormatCurrency(decimal amount) => $"{amount.ToString("C", CustomCulture).Trim()} Eur";
    }
}