using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.PosCharges;
using Quivi.Application.Services.Exceptions;
using Quivi.Backoffice.Api.Requests.Transactions;
using Quivi.Backoffice.Api.Responses.Transactions;
using Quivi.Backoffice.Api.Validations;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Parameters;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;
        private readonly IAcquirerChargeProcessor chargeProcessor;

        public TransactionsController(IQueryProcessor queryProcessor,
                                        IMapper mapper,
                                        IIdConverter idConverter,
                                        IAcquirerChargeProcessor chargeProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
            this.chargeProcessor = chargeProcessor;
        }

        [HttpGet]
        public async Task<GetTransactionsResponse> Get([FromQuery] GetTransactionsRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var result = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),

                //TODO: implement search
                //Search = request.Search,

                //TODO: implement overpayed
                //OverPayed = request.OverPayed,

                FromCapturedDate = request.FromDate?.ToUniversalTime(),
                ToCapturedDate = request.ToDate?.ToUniversalTime(),
                SyncingState = Convert(request.SyncState),
                HasDiscounts = request.HasDiscounts,
                HasReviewComment = request.HasReviewComment,
                IsCaptured = true,
                CustomChargeMethodIds = string.IsNullOrWhiteSpace(request.CustomChargeMethodId) ? null : [idConverter.FromPublicId(request.CustomChargeMethodId)],
                QuiviPaymentsOnly = request.QuiviPaymentsOnly,
                HasRefunds = request.HasRefunds,

                IncludePosChargeInvoiceItemsOrderMenuItems = true,
                IncludeMerchantCustomCharge = true,
                IncludePosChargeSyncAttempts = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetTransactionsResponse
            {
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
                Data = mapper.Map<Dtos.Transaction>(result),
            };
        }

        [HttpPost("{id}/refund")]
        public async Task<RefundTransactionResponse> Refund(string id, [FromBody] RefundTransactionRequest request)
        {
            try
            {
                await chargeProcessor.Refund(new RefundParameters
                {
                    Amount = request.Amount,
                    ChargeId = idConverter.FromPublicId(id),
                    MerchantId = User.IsAdmin() ? null : User.SubMerchantId(idConverter)!.Value,
                    IsCancellation = request.Cancelation,
                });

                var result = await queryProcessor.Execute(new GetPosChargesAsyncQuery
                {
                    ParentMerchantIds = User.IsAdmin() ? null : [User.MerchantId(idConverter)!.Value],
                    MerchantIds = User.IsAdmin() ? null : [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],

                    IncludePosChargeInvoiceItemsOrderMenuItems = true,
                    IncludeMerchantCustomCharge = true,
                    IncludePosChargeSyncAttempts = true,

                    PageIndex = 0,
                    PageSize = 1,
                });

                return new RefundTransactionResponse
                {
                    Data = mapper.Map<Dtos.Transaction>(result.SingleOrDefault()),
                };
            }
            catch (Exception ex)
            {
                if (ex is NoBalanceException e)
                {
                    using var validator = new ModelStateValidator<string, ValidationError>(id);
                    validator.AddError(e => e, ValidationError.NoBalance);
                    throw validator.Exception;
                }

                throw;
            }
        }

        private SyncAttemptState? Convert(Dtos.SynchronizationState? synchronizationState)
        {
            if (synchronizationState == null)
                return null;

            switch (synchronizationState.Value)
            {
                case Dtos.SynchronizationState.Failed: return SyncAttemptState.Failed;
                case Dtos.SynchronizationState.Succeeded: return SyncAttemptState.Synced;
                case Dtos.SynchronizationState.Syncing: return SyncAttemptState.Syncing;
            }
            return null;
        }
    }
}