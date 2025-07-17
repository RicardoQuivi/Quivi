using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.ChannelProfiles;
using Quivi.Guests.Api.Dtos.Responses.ChannelProfiles;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelProfilesController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ChannelProfilesController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<GetChannelProfileResponse> Get(string id)
        {
            var result = await queryProcessor.Execute(new GetChannelProfilesAsyncQuery
            {
                Ids = [idConverter.FromPublicId(id)],
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetChannelProfileResponse
            {
                Data = mapper.Map<Dtos.ChannelProfile>(result.FirstOrDefault()),
            };
        }
    }
}