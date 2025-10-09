using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Availabilities;
using Quivi.Application.Commands.ChannelProfiles;
using Quivi.Application.Queries.AvailabilityGroupChannelProfileAssociations;
using Quivi.Backoffice.Api.Requests.AvailabilityChannelProfileAssociations;
using Quivi.Backoffice.Api.Responses.AvailabilityChannelProfileAssociations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireSubMerchant]
    public class AvailabilityChannelProfileAssociationsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public AvailabilityChannelProfileAssociationsController(IQueryProcessor queryProcessor,
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
        public async Task<GetAvailabilityChannelProfileAssociationsResponse> Get([FromQuery] GetAvailabilityChannelProfileAssociationsRequest request)
        {
            request ??= new();

            var result = await queryProcessor.Execute(new GetAvailabilityGroupChannelProfileAssociationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                AvailabilityGroupIds = request.AvailabilityIds?.Select(idConverter.FromPublicId),
                ChannelProfileIds = request.ChannelProfileIds?.Select(idConverter.FromPublicId),

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetAvailabilityChannelProfileAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityChannelProfileAssociation>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPatch("/api/Availabilities/{id}/ChannelProfileAssociations")]
        public async Task<UpdateAvailabilityChannelProfileAssociationsResponse> PutViaAvailability(string id, UpdateAvailabilityChannelProfileAssociationsRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateAvailabilityGroupsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetAvailabilityGroupsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
                UpdateAction = (model) =>
                {
                    foreach (var association in request.Associations)
                    {
                        var channelProfileId = idConverter.FromPublicId(association.Id);
                        if (association.Active)
                        {
                            model.ChannelProfiles.Upsert(channelProfileId, t =>
                            {

                            });
                            continue;
                        }

                        if (model.ChannelProfiles.ContainsKey(channelProfileId))
                            model.ChannelProfiles.Remove(channelProfileId);
                    }
                    return Task.CompletedTask;
                },
                OnInvalidName = (e) => { },
            });

            return new UpdateAvailabilityChannelProfileAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityChannelProfileAssociation>(result.SelectMany(s => s.AssociatedChannelProfiles!)),
            };
        }

        [HttpPatch("/api/ChannelProfiles/{id}/AvailabilityAssociations")]
        public async Task<UpdateAvailabilityChannelProfileAssociationsResponse> PutViaChannelProfile(string id, UpdateAvailabilityChannelProfileAssociationsRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateChannelProfileAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetChannelProfilesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
                UpdateAction = (model) =>
                {
                    foreach (var association in request.Associations)
                    {
                        var orderConfigurableField = idConverter.FromPublicId(association.Id);
                        if (association.Active)
                        {
                            model.AvailabilityGroups.Upsert(orderConfigurableField, t =>
                            {

                            });
                            continue;
                        }

                        if (model.AvailabilityGroups.ContainsKey(orderConfigurableField))
                            model.AvailabilityGroups.Remove(orderConfigurableField);
                    }
                    return Task.CompletedTask;
                },
            });

            return new UpdateAvailabilityChannelProfileAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityChannelProfileAssociation>(result.SelectMany(s => s.AssociatedAvailabilityGroups!)),
            };
        }
    }
}