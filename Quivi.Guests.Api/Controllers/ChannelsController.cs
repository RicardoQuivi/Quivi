using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.Channels;
using Quivi.Guests.Api.Dtos.Responses.Channels;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ChannelsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<GetChannelResponse> Get(string id)
        {
            var result = await queryProcessor.Execute(new GetChannelsAsyncQuery
            {
                Ids = [idConverter.FromPublicId(id)],
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetChannelResponse
            {
                Data = mapper.Map<Dtos.Channel>(result.FirstOrDefault()),
            };
        }
    }
}