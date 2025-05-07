using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.ChannelProfiles;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.ChannelProfiles;
using Quivi.Pos.Api.Dtos.Responses.ChannelProfiles;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [RequireEmployee]
    [ApiController]
    public class ChannelProfilesController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public ChannelProfilesController(IQueryProcessor queryProcessor,
                                            ICommandProcessor commandProcessor,
                                            IIdConverter idConverter,
                                            IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetChannelProfilesResponse> Get([FromQuery] GetChannelProfilesRequest request)
        {
            request ??= new();
            var result = await queryProcessor.Execute(new GetChannelProfilesAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Flags = request.AllowsSessionsOnly ? ChannelFeature.AllowsSessions : null,
                HasChannels = request.HasChannels,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetChannelProfilesResponse
            {
                Data = mapper.Map<Dtos.ChannelProfile>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }
    }
}