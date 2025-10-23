using ComplyPay.Abstractions;
using ComplyPay.Dtos;
using ComplyPay.Dtos.Requests;
using ComplyPay.Exceptions;
using Quivi.Infrastructure.Abstractions.Payouts;

namespace Quivi.Infrastructure.Payouts.ComplyPay
{
    public class ComplyPayApiPayoutProcessor : IPayoutProcessor
    {
        private readonly IComplyPayService service;
        private readonly IComplyPaySettings complyPaySettings;

        public ComplyPayApiPayoutProcessor(IComplyPayService complyPayService, IComplyPaySettings complyPaySettings)
        {
            this.service = complyPayService;
            this.complyPaySettings = complyPaySettings;
        }

        public async Task Process(IEnumerable<Payout> payouts)
        {
            if (complyPaySettings.SkipPayouts)
                return;

            decimal total = 0.0M;

            var merchantIds = payouts.Select(s => s.MerchantId.ToString()).ToList();
            var vendorsResponse = await service.GetVendorAccounts(new GetVendorAccountsRequest
            {
                ReferenceIds = merchantIds,
                Page = 0,
                PageSize = merchantIds.Count,
            });
            var vendorsDictionary = vendorsResponse.VendorAccounts.ToDictionary(v => v.ReferenceId, v => v);

            foreach (var row in payouts)
                if (await ProcessPayout(row, vendorsDictionary))
                    total += row.TransferAmount;

            await MoveRemainingToTreasury(total);
        }

        private async Task<bool> ProcessPayout(Payout row, IReadOnlyDictionary<string, VendorAccount> vendorsDictionary)
        {
            if (row.TransferAmount == 0)
                return false;

            if (row.TransferAmount < 0)
                return false;

            try
            {
                var vendor = vendorsDictionary[row.MerchantId.ToString()];
                var integerAmountToTransfer = (int)(row.TransferAmount * 100);

                try
                {
                    var memo = $"QV-{row.TransferReference}";
                    var paymentResponse = await service.CreatePayment(new CreatePaymentRequest
                    {
                        IdempotencyKey = $"{memo}-{row.MerchantId}",
                        Amount = integerAmountToTransfer,
                        Currency = "EUR",
                        Memo = memo,
                        Payer = new Payer
                        {
                            Type = AccountType.Split,
                        },
                        Payee = new Payee
                        {
                            Id = vendor.AccountIdentifier,
                        },
                        PaymentFlowType = PaymentFlowType.PlatformNo4Eyes,
                    });

                }
                catch (PaymentAlreadyExistsException)
                {
                }
                return true;
            }
            catch (KeyNotFoundException)
            {
                throw new Exception($"Merchant {row.MerchantId} with name {row.Name} does not exist in ComplyPay!");
            }
        }

        private async Task MoveRemainingToTreasury(decimal totalTransfered)
        {
            if (string.IsNullOrWhiteSpace(complyPaySettings.TreasuryId))
                return;

            var splitBalanceResponse = await service.GetWalletBallance(new GetWalletBallanceRequest
            {
                AccountType = AccountType.Split,
            });

            var amountToTransfer = splitBalanceResponse.Balance.Amount - (int)(totalTransfered * 100);

            if (amountToTransfer <= 0)
                return;

            try
            {
                var paymentResponse = await service.CreatePayment(new CreatePaymentRequest
                {
                    Amount = amountToTransfer,
                    Currency = "EUR",
                    Payer = new Payer
                    {
                        Type = AccountType.Split,
                    },
                    Payee = new Payee
                    {
                        Id = complyPaySettings.TreasuryId,
                    },
                    PaymentFlowType = PaymentFlowType.PlatformNo4Eyes,
                });
            }
            catch (PaymentAlreadyExistsException)
            {
            }
        }
    }
}
