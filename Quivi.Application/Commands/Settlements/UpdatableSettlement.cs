using Quivi.Domain.Entities.Financing;

namespace Quivi.Application.Commands.Settlements
{
    public class UpdatableSettlement : IUpdatableSettlement
    {
        private class UpdatableSettlementDetail : IUpdatableSettlementDetail
        {
            public readonly SettlementDetail Model;
            private readonly bool isNew;

            private readonly decimal originalAmount;
            private readonly decimal originalIncludedTip;
            private readonly string originalMerchantIban;
            private readonly decimal originalMerchantVatRate;
            private readonly decimal originalTransactionFee;
            private readonly decimal originalFeeAmount;
            private readonly decimal originalVatAmount;
            private readonly decimal originalNetAmount;
            private readonly decimal originalIncludedNetTip;
            private readonly int originalParentMerchantId;
            private readonly int originalMerchantId;

            public UpdatableSettlementDetail(SettlementDetail model)
            {
                this.Model = model;
                isNew = model.SettlementId == 0;

                originalAmount = Model.Amount;
                originalIncludedTip = Model.IncludedTip;
                originalMerchantIban = Model.MerchantIban;
                originalMerchantVatRate = Model.MerchantVatRate;
                originalTransactionFee = Model.TransactionFee;
                originalFeeAmount = Model.FeeAmount;
                originalVatAmount = Model.VatAmount;
                originalNetAmount = Model.NetAmount;
                originalIncludedNetTip = Model.IncludedNetTip;
                originalParentMerchantId = Model.ParentMerchantId;
                originalMerchantId = Model.MerchantId;
            }

            public int JournalId => Model.JournalId;

            public decimal Amount { get => Model.Amount; set => Model.Amount = value; }
            public decimal IncludedTip { get => Model.IncludedTip; set => Model.IncludedTip = value; }
            public string MerchantIban { get => Model.MerchantIban; set => Model.MerchantIban = value; }
            public decimal MerchantVatRate { get => Model.MerchantVatRate; set => Model.MerchantVatRate = value; }
            public decimal TransactionFee { get => Model.TransactionFee; set => Model.TransactionFee = value; }
            public decimal FeeAmount { get => Model.FeeAmount; set => Model.FeeAmount = value; }
            public decimal VatAmount { get => Model.VatAmount; set => Model.VatAmount = value; }
            public decimal NetAmount { get => Model.NetAmount; set => Model.NetAmount = value; }
            public decimal IncludedNetTip { get => Model.IncludedNetTip; set => Model.IncludedNetTip = value; }
            public int ParentMerchantId { get => Model.ParentMerchantId; set => Model.ParentMerchantId = value; }
            public int MerchantId { get => Model.MerchantId; set => Model.MerchantId = value; }

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalAmount != Model.Amount)
                        return true;

                    if (originalIncludedTip != Model.IncludedTip)
                        return true;

                    if (originalMerchantIban != Model.MerchantIban)
                        return true;

                    if (originalMerchantVatRate != Model.MerchantVatRate)
                        return true;

                    if (originalTransactionFee != Model.TransactionFee)
                        return true;

                    if (originalFeeAmount != Model.FeeAmount)
                        return true;

                    if (originalVatAmount != Model.VatAmount)
                        return true;

                    if (originalNetAmount != Model.NetAmount)
                        return true;

                    if (originalIncludedNetTip != Model.IncludedNetTip)
                        return true;

                    if (originalParentMerchantId != Model.ParentMerchantId)
                        return true;

                    if (originalMerchantId != Model.MerchantId)
                        return true;

                    return false;
                }
            }
        }

        private class UpdatableSettlementServiceDetail : IUpdatableSettlementServiceDetail
        {
            public readonly SettlementServiceDetail Model;
            private readonly bool isNew;

            private readonly int originalMerchantServiceId;
            private readonly int originalParentMerchantId;
            private readonly int originalMerchantId;
            private readonly string originalMerchantIban;
            private readonly decimal originalMerchantVatRate;
            private readonly decimal originalAmount;
            private readonly decimal originalVatAmount;

            public UpdatableSettlementServiceDetail(SettlementServiceDetail model)
            {
                this.Model = model;
                isNew = model.SettlementId == 0;

                originalMerchantServiceId = model.MerchantServiceId;
                originalParentMerchantId = Model.ParentMerchantId;
                originalMerchantId = Model.MerchantId;
                originalMerchantIban = Model.MerchantIban ?? string.Empty;
                originalMerchantVatRate = Model.MerchantVatRate;
                originalAmount = Model.Amount;
                originalVatAmount = Model.VatAmount;
            }

            public int JournalId => Model.JournalId;

            public int MerchantServiceId { get => Model.MerchantServiceId; set => Model.MerchantServiceId = value; }
            public int ParentMerchantId { get => Model.ParentMerchantId; set => Model.ParentMerchantId = value; }
            public int MerchantId { get => Model.MerchantId; set => Model.MerchantId = value; }
            public string MerchantIban { get => Model.MerchantIban ?? string.Empty; set => Model.MerchantIban = value; }
            public decimal MerchantVatRate { get => Model.MerchantVatRate; set => Model.MerchantVatRate = value; }
            public decimal Amount { get => Model.Amount; set => Model.Amount = value; }
            public decimal VatAmount { get => Model.VatAmount; set => Model.VatAmount = value; }

            public bool HasChanges
            {
                get
                {
                    if (isNew)
                        return true;

                    if (originalMerchantServiceId != Model.MerchantServiceId)
                        return true;

                    if (originalAmount != Model.Amount)
                        return true;

                    if (originalMerchantIban != Model.MerchantIban)
                        return true;

                    if (originalMerchantVatRate != Model.MerchantVatRate)
                        return true;

                    if (originalVatAmount != Model.VatAmount)
                        return true;

                    if (originalParentMerchantId != Model.ParentMerchantId)
                        return true;

                    if (originalMerchantId != Model.MerchantId)
                        return true;

                    return false;
                }
            }
        }

        public readonly Settlement Model;
        private readonly bool isNew;

        private readonly SettlementState originalState;

        public readonly UpdatableRelationshipEntity<SettlementDetail, IUpdatableSettlementDetail, int> UpdatableSettlementDetails;
        public readonly UpdatableRelationshipEntity<SettlementServiceDetail, IUpdatableSettlementServiceDetail, int> UpdatableSettlementServiceDetails;

        public UpdatableSettlement(Settlement model, DateTime now)
        {
            Model = model;
            this.isNew = model.Id == 0;
            this.UpdatableSettlementDetails = new UpdatableRelationshipEntity<SettlementDetail, IUpdatableSettlementDetail, int>(model.SettlementDetails!, m => m.JournalId, t => new UpdatableSettlementDetail(t), (id) => new SettlementDetail
            {
                JournalId = id,

                Settlement = this.Model,
                SettlementId = this.Model.Id,

                MerchantIban = string.Empty,

                CreatedDate = now,
                ModifiedDate = now,
            });
            this.UpdatableSettlementServiceDetails = new UpdatableRelationshipEntity<SettlementServiceDetail, IUpdatableSettlementServiceDetail, int>(model.SettlementServiceDetails!, m => m.JournalId, t => new UpdatableSettlementServiceDetail(t), (id) => new SettlementServiceDetail
            {
                JournalId = id,

                Settlement = this.Model,
                SettlementId = this.Model.Id,

                MerchantIban = string.Empty,

                CreatedDate = now,
                ModifiedDate = now,
            });
        }

        public int Id => Model.Id;
        public DateOnly Date => DateOnly.FromDateTime(Model.Date);

        public SettlementState State
        {
            get => Model.State;
            set => Model.State = value;
        }

        public IUpdatableRelationship<IUpdatableSettlementDetail, int> SettlementDetails => UpdatableSettlementDetails;
        public IUpdatableRelationship<IUpdatableSettlementServiceDetail, int> SettlementServiceDetails => UpdatableSettlementServiceDetails;

        public bool HasChanges
        {
            get
            {
                if (isNew)
                    return true;

                if (originalState != Model.State)
                    return true;

                if (UpdatableSettlementDetails.HasChanges)
                    return true;

                if (UpdatableSettlementServiceDetails.HasChanges)
                    return true;

                return false;
            }
        }
    }
}