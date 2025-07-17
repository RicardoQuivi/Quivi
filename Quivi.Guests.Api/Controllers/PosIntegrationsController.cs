using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Guests.Api.Dtos.Responses.PosIntegrations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PosIntegrationsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public PosIntegrationsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<GetPosIntegrationResponse> Get(string id)
        {
            var result = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = [idConverter.FromPublicId(id)],
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetPosIntegrationResponse
            {
                Data = mapper.Map<Dtos.PosIntegration>(result.FirstOrDefault()),
            };
        }
    }
}
