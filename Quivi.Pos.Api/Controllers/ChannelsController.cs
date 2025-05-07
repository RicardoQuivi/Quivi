using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.Channels;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Channels;
using Quivi.Pos.Api.Dtos.Responses.Channels;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [RequireEmployee]
    [ApiController]
    public class ChannelsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public ChannelsController(IQueryProcessor queryProcessor,
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
        public async Task<GetChannelsResponse> Get([FromQuery] GetChannelsRequest request)
        {
            ChannelFeature? features = null;
            if (request.AllowsSessionsOnly)
            {
                features = features ?? ChannelFeature.None;
                features |= ChannelFeature.AllowsSessions;
            }
            if (request.AllowsPrePaidOrderingOnly)
            {
                features = features ?? ChannelFeature.None;
                features |= ChannelFeature.AllowsOrderAndPay;
            }
            if (request.AllowsPostPaidOrderingOnly)
            {
                features = features ?? ChannelFeature.None;
                features |= ChannelFeature.AllowsPostPaymentOrdering;
            }

            var result = await queryProcessor.Execute(new GetChannelsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                SessionIds = request.SessionIds?.Select(idConverter.FromPublicId),
                ChannelProfileIds = string.IsNullOrWhiteSpace(request.ChannelProfileId) ? null : [idConverter.FromPublicId(request.ChannelProfileId)],
                Flags = features,
                IsDeleted = request.IncludeDeleted ? null : false,
                Search = request.Search,
                HasOpenSession = request.HasOpenSession,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            var response = new GetChannelsResponse
            {
                Data = mapper.Map<Dtos.Channel>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };

            return response;
        }
    }
}