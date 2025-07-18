using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.PosCharges;
using Quivi.Backoffice.Api.Requests.Transactions;
using Quivi.Backoffice.Api.Responses.Transactions;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public TransactionsController(IQueryProcessor queryProcessor,
                                        ICommandProcessor commandProcessor,
                                        IMapper mapper,
                                        IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
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