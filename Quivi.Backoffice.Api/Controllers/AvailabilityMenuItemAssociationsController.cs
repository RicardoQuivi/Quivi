using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Availabilities;
using Quivi.Application.Commands.MenuItems;
using Quivi.Application.Queries.AvailabilityGroupMenuItemAssociations;
using Quivi.Backoffice.Api.Requests.AvailabilityMenuItemAssociations;
using Quivi.Backoffice.Api.Responses.AvailabilityMenuItemAssociations;
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
    public class AvailabilityMenuItemAssociationsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public AvailabilityMenuItemAssociationsController(IQueryProcessor queryProcessor,
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
        public async Task<GetAvailabilityMenuItemAssociationsResponse> Get([FromQuery] GetAvailabilityMenuItemAssociationsRequest request)
        {
            request ??= new();

            var result = await queryProcessor.Execute(new GetAvailabilityGroupMenuItemAssociationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                AvailabilityGroupIds = request.AvailabilityIds?.Select(idConverter.FromPublicId),
                MenuItemIds = request.MenuItemIds?.Select(idConverter.FromPublicId),

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetAvailabilityMenuItemAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityMenuItemAssociation>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPatch("/api/Availabilities/{id}/MenuItemAssociations")]
        public async Task<UpdateAvailabilityMenuItemAssociationsResponse> PutViaAvailability(string id, UpdateAvailabilityMenuItemAssociationsRequest request)
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
                            model.MenuItems.Upsert(channelProfileId, t =>
                            {

                            });
                            continue;
                        }

                        if (model.MenuItems.ContainsKey(channelProfileId))
                            model.MenuItems.Remove(channelProfileId);
                    }
                    return Task.CompletedTask;
                },
                OnInvalidName = (e) => { },
            });

            return new UpdateAvailabilityMenuItemAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityMenuItemAssociation>(result.SelectMany(s => s.AssociatedMenuItems!)),
            };
        }

        [HttpPatch("/api/MenuItems/{id}/AvailabilityAssociations")]
        public async Task<UpdateAvailabilityMenuItemAssociationsResponse> PutViaChannelProfile(string id, UpdateAvailabilityMenuItemAssociationsRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateMenuItemsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetMenuItemsCriteria
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

            return new UpdateAvailabilityMenuItemAssociationsResponse
            {
                Data = mapper.Map<Dtos.AvailabilityMenuItemAssociation>(result.SelectMany(s => s.AssociatedAvailabilityGroups!)),
            };
        }
    }
}