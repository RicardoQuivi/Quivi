﻿using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class TransactionMapperHandler : IMapperHandler<PosCharge, Dtos.Transaction>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public TransactionMapperHandler(IIdConverter idConverter, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public Transaction Map(PosCharge model)
        {
            SyncStatus state = SyncStatus.Failed;
            decimal syncedAmount = 0.0m;
            foreach(var syncAttempt in model.PosChargeSyncAttempts ?? [])
            {
                if(syncAttempt.State == SyncAttemptState.Synced)
                {
                    state = SyncStatus.Synced;
                    syncedAmount = syncAttempt.SyncedAmount;
                    break;
                }

                if(syncAttempt.State == SyncAttemptState.Syncing)
                    state = SyncStatus.Syncing;
            }

            return new Transaction
            {
                Id = idConverter.ToPublicId(model.Id),
                Total = model.Total,
                Payment = model.Payment,
                Tip = model.Tip,
                Surcharge = model.SurchargeFeeAmount,
                SyncedAmount = syncedAmount,
                Status = this.mapper.Map<TransactionStatus>(model.Charge?.Status ?? ChargeStatus.Processing),
                Method = model.Charge?.ChargeMethod ?? ChargeMethod.Custom,
                CapturedDate = model.CaptureDate.HasValue ? new DateTimeOffset(model.CaptureDate.Value, TimeSpan.Zero) : null,
                SyncStatus = state
            };
        }
    }
}