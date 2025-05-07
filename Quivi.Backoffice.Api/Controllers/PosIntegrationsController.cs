using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Backoffice.Api.Requests.PosIntegrations;
using Quivi.Backoffice.Api.Responses.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireSubMerchant]
    [Authorize]
    public class PosIntegrationsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public PosIntegrationsController(IQueryProcessor queryProcessor,
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
        public async Task<GetPosIntegrationsResponse> Get([FromQuery] GetPosIntegrationsRequest request)
        {
            request ??= new GetPosIntegrationsRequest();

            var result = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ChannelIds = string.IsNullOrWhiteSpace(request.ChannelId) ? null : [ idConverter.FromPublicId(request.ChannelId) ],
                MerchantIds = [ User.SubMerchantId(idConverter)!.Value ],
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPosIntegrationsResponse
            {
                Data = mapper.Map<Dtos.PosIntegration>(result),
            };
        }
    }
}
