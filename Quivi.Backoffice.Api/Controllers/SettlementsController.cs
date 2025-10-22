using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.Settlements;
using Quivi.Backoffice.Api.Requests.Settlements;
using Quivi.Backoffice.Api.Responses.Settlements;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireMerchant]
    public class SettlementsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public SettlementsController(IQueryProcessor queryProcessor, IIdConverter idConverter, IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetSettlementsResponse> Get([FromQuery] GetSettlementsRequest request)
        {
            var submerchantId = User.SubMerchantId(idConverter);
            var result = await queryProcessor.Execute(new GetMerchantSettlementResumesAsyncQuery
            {
                SettlementIds = request.Ids?.Select(idConverter.FromPublicId),
                ParentMerchantIds = [User.MerchantId(idConverter)!.Value],
                MerchantIds = submerchantId.HasValue ? [submerchantId.Value] : null,
                ChargeMethods = request.ChargeMethod.HasValue ? [request.ChargeMethod.Value] : null,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetSettlementsResponse
            {
                Data = mapper.Map<Dtos.Settlement>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpGet("{id}/details")]
        public async Task<GetSettlementDetailsResponse> GetDetails(string id, [FromQuery] GetSettlementDetailsRequest request)
        {
            var submerchantId = User.SubMerchantId(idConverter);
            var result = await queryProcessor.Execute(new GetMerchantSettlementDetailsAsyncQuery
            {
                SettlementIds = [idConverter.FromPublicId(id)],
                ParentMerchantIds = [User.MerchantId(idConverter)!.Value],
                MerchantIds = submerchantId.HasValue ? [submerchantId.Value] : null,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetSettlementDetailsResponse
            {
                Data = mapper.Map<Dtos.SettlementDetail>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }
    }
}
