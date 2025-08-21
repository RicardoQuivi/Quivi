using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Channels;
using Quivi.Application.Queries.Channels;
using Quivi.Backoffice.Api.Requests.Channels;
using Quivi.Backoffice.Api.Responses.Channels;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [ApiController]
    [Authorize]
    public class ChannelsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ChannelsController(IQueryProcessor queryProcessor,
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
        public async Task<GetChannelsResponse> Get([FromQuery] GetChannelsRequest request)
        {
            request ??= new GetChannelsRequest();

            ChannelFeature? features = null;
            if (request.AllowsSessionsOnly)
            {
                features ??= ChannelFeature.None;
                features |= ChannelFeature.AllowsSessions;
            }

            var subMerchantId = User.SubMerchantId(idConverter);
            if (User.IsAdmin() == false && subMerchantId == null)
                throw new UnauthorizedAccessException();

            var result = await queryProcessor.Execute(new GetChannelsAsyncQuery
            {
                MerchantIds = subMerchantId == null ? null : [subMerchantId.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                Flags = features,
                PageIndex = request.Page,
                PageSize = request.PageSize,
                IsDeleted = false,
            });

            return new GetChannelsResponse
            {
                Data = mapper.Map<Dtos.Channel>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateChannelsResponse> Create([FromBody] CreateChannelsRequest request)
        {
            if (string.IsNullOrWhiteSpace(User.SubMerchantId()))
                throw new UnauthorizedAccessException();

            var result = await commandProcessor.Execute(new AddChannelsAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                ChannelProfileId = idConverter.FromPublicId(request.ChannelProfileId),
                Data = request.Data.Select(item => new ChannelToAdd
                {
                    Identifier = item.Name,
                }),
            });

            return new CreateChannelsResponse
            {
                Data = mapper.Map<Dtos.Channel>(result),
            };
        }

        [HttpPatch]
        public async Task<PatchChannelsResponse> Patch([FromBody] PatchChannelsRequest request)
        {
            if (string.IsNullOrWhiteSpace(User.SubMerchantId()))
                throw new UnauthorizedAccessException();

            var result = await commandProcessor.Execute(new UpdateChannelAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetChannelsCriteria
                {
                    Ids = request.Ids.Select(idConverter.FromPublicId),
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
                UpdateAction = (e) =>
                {
                    if (request.Ids != null && request.Ids.Skip(1).Any() == false)
                    {
                        if (string.IsNullOrWhiteSpace(request.Name) == false)
                            e.Identifier = request.Name;
                    }

                    if (request.ChannelProfileId != null)
                        e.ChannelProfileId = idConverter.FromPublicId(request.ChannelProfileId);

                    return Task.CompletedTask;
                }
            });

            return new PatchChannelsResponse
            {
                Data = mapper.Map<Dtos.Channel>(result),
            };
        }

        [HttpDelete]
        public async Task<DeleteChannelsResponse> Delete([FromBody] DeleteChannelsRequest request)
        {
            if (string.IsNullOrWhiteSpace(User.SubMerchantId()))
                throw new UnauthorizedAccessException();

            var result = await commandProcessor.Execute(new UpdateChannelAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetChannelsCriteria
                {
                    Ids = request.Ids.Select(idConverter.FromPublicId),
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
                UpdateAction = (e) =>
                {
                    e.IsDeleted = true;
                    return Task.CompletedTask;
                }
            });

            return new DeleteChannelsResponse();
        }

        [HttpPost("qrcodes")]
        public async Task<GenerateChannelQrCodesResponse> GenerateQrCode([FromBody] GenerateChannelQrCodesRequest request)
        {
            var pdf = await commandProcessor.Execute(new GenerateChannelsQrCodesAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                ChannelIds = request.ChannelIds?.Select(idConverter.FromPublicId),
                MainText = request.MainText,
                SecondaryText = request.SecondaryText,
            });

            return new GenerateChannelQrCodesResponse
            {
                Base64Content = Convert.ToBase64String(pdf),
            };
        }
    }
}