using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.ChannelProfiles;
using Quivi.Application.Queries.ChannelProfiles;
using Quivi.Backoffice.Api.Requests.ChannelProfiles;
using Quivi.Backoffice.Api.Responses.ChannelProfiles;
using Quivi.Backoffice.Api.Validations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireSubMerchant]
    [Authorize]
    public class ChannelProfilesController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ChannelProfilesController(IQueryProcessor queryProcessor,
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
        public async Task<GetChannelProfilesResponse> Get([FromQuery] GetChannelProfilesRequest request)
        {
            request ??= new GetChannelProfilesRequest();

            var result = await queryProcessor.Execute(new GetChannelProfilesAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                ChannelIds = request.ChannelIds?.Select(idConverter.FromPublicId),
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
                IsDeleted = false, 
            });

            return new GetChannelProfilesResponse
            {
                Data = mapper.Map<Dtos.ChannelProfile>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateChannelProfileResponse> Create([FromBody] CreateChannelProfileRequest request)
        {
            var result = await commandProcessor.Execute(new AddChannelProfileAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Features = request.Features,
                MinimumPrePaidOrderAmount = request.MinimumPrePaidOrderAmount,
                Name = request.Name,
                SendToPreparationTimer = request.SendToPreparationTimer,
                PosIntegrationId = idConverter.FromPublicId(request.PosIntegrationId),
            });

            return new CreateChannelProfileResponse
            {
                Data = mapper.Map<Dtos.ChannelProfile>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchChannelProfileResponse> Patch(string id, [FromBody] PatchChannelProfileRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateChannelProfileAsyncCommand
            {
                Criteria = new GetChannelProfilesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageIndex = 0,
                    PageSize = 1,
                    IsDeleted = false,
                },
                UpdateAction = (e) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        e.Name = request.Name;

                    if (request.MinimumPrePaidOrderAmount.HasValue)
                        e.MinimumPrePaidOrderAmount = request.MinimumPrePaidOrderAmount.Value;

                    if (request.SendToPreparationTimerWasSet)
                        e.SendToPreparationTimer = request.SendToPreparationTimer;

                    if (request.Features.HasValue)
                        e.Features = request.Features.Value;

                    if (request.PosIntegrationId != null)
                        e.PosIntegrationId = idConverter.FromPublicId(request.PosIntegrationId);

                    return Task.CompletedTask;
                }
            });

            return new PatchChannelProfileResponse
            {
                Data = mapper.Map<Dtos.ChannelProfile>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteChannelProfileResponse> Delete(string id)
        {
            using var validator = new ModelStateValidator<string, ValidationError>(id);
            var result = await commandProcessor.Execute(new DeleteChannelProfileAsyncCommand
            {
                Criteria = new GetChannelProfilesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageIndex = 0,
                    PageSize = 1,
                },
                OnChannelsAssociatedError = (e) => validator.AddError(e => e, ValidationError.UnableToDeleteDueToAssociatedEntities),
            });
            return new DeleteChannelProfileResponse();
        }
    }
}