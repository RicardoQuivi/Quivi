using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Sessions;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Sessions;
using Quivi.Pos.Api.Dtos.Responses.Sessions;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [RequireSubMerchant]
    [Authorize]
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

        [HttpGet]
        public async Task<GetSessionsResponse> Get([FromQuery] GetSessionsRequest request)
        {
            IEnumerable<SessionStatus> states = [SessionStatus.Closed, SessionStatus.Ordering];
            var sessionsQuery = await queryProcessor.Execute(new GetSessionsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ChannelIds = request.ChannelIds?.Select(idConverter.FromPublicId),
                LatestSessionsOnly = request.Ids == null,
                Statuses = request.IncludeDeleted ? states.Append(SessionStatus.Unknown) : states,
                IncludeOrdersMenuItems = true,
                IncludeOrdersMenuItemsPosChargeInvoiceItems = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetSessionsResponse
            {
                Data = mapper.Map<Dtos.Session>(sessionsQuery),
                Page = sessionsQuery.CurrentPage,
                TotalItems = sessionsQuery.TotalItems,
                TotalPages = sessionsQuery.NumberOfPages,
            };
        }
    }
}
