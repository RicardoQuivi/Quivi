using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.OrderConfigurableFields;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos;
using Quivi.Pos.Api.Dtos.Requests.ConfigurableFields;
using Quivi.Pos.Api.Dtos.Responses.ConfigurableFields;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class ConfigurableFieldsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ConfigurableFieldsController(IQueryProcessor queryProcessor, IIdConverter idConverter, IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetConfigurableFieldsResponse> Get([FromQuery] GetConfigurableFieldsRequest request)
        {
            request = request ?? new GetConfigurableFieldsRequest();

            var query = await queryProcessor.Execute(new GetOrderConfigurableFieldsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                ChannelIds = request.ChannelIds?.Select(idConverter.FromPublicId),
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ForPosSessions = request.ForPosSessions,
                IsAutoFill = false,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetConfigurableFieldsResponse
            {
                Data = mapper.Map<ConfigurableField>(query),
            };
        }
    }
}