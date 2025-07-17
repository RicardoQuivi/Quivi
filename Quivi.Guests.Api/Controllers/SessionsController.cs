using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Sessions;
using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos.Responses.Sessions;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public SessionsController(IQueryProcessor queryProcessor,
                            ICommandProcessor commandProcessor,
                            IIdConverter idConverter,
                            IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        [HttpGet("{channelId}")]
        public async Task<GetSessionResponse> Get(string channelId)
        {
            var sessionsQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
            {
                ChannelIds = [idConverter.FromPublicId(channelId)],
                LatestSessionsOnly = true,
                Statuses = [SessionStatus.Ordering],
                IncludeOrdersMenuItems = true,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetSessionResponse
            {
                Data = mapper.Map<Dtos.Session>(sessionsQuery.FirstOrDefault()),
            };
        }
    }
}