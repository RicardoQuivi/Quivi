using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.ChannelProfiles;
using Quivi.Application.Commands.OrderConfigurableFields;
using Quivi.Application.Queries.OrderConfigurableFieldChannelProfileAssociations;
using Quivi.Backoffice.Api.Requests.ConfigurableFieldAssociations;
using Quivi.Backoffice.Api.Responses.ConfigurableFieldAssociations;
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
    public class ConfigurableFieldAssociationsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public ConfigurableFieldAssociationsController(IQueryProcessor queryProcessor,
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
        public async Task<GetConfigurableFieldAssociationsResponse> Get([FromQuery] GetConfigurableFieldAssociationsRequest request)
        {
            request ??= new();

            var result = await queryProcessor.Execute(new GetOrderConfigurableFieldChannelProfileAssociationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                OrderConfigurableFieldIds = request.ConfigurableFieldIds?.Select(idConverter.FromPublicId),
                ChannelProfileIds = request.ChannelProfileIds?.Select(idConverter.FromPublicId),

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetConfigurableFieldAssociationsResponse
            {
                Data = mapper.Map<Dtos.ConfigurableFieldAssociation>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPatch("/api/ConfigurableFields/{id}/Associations")]
        public async Task<UpdateConfigurableFieldAssociationsResponse> PutViaConfigurableField(string id, UpdateConfigurableFieldAssociationsRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateOrderConfigurableFieldsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetOrderConfigurableFieldsCriteria
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
                OnAutoFillWithEmptyDefaultValue = () => { },
                OnInvalidDefaultValue = () => { },
            });

            return new UpdateConfigurableFieldAssociationsResponse
            {
                Data = mapper.Map<Dtos.ConfigurableFieldAssociation>(result.SelectMany(s => s.AssociatedChannelProfiles!)),
            };
        }

        [HttpPatch("/api/ChannelProfiles/{id}/Associations")]
        public async Task<UpdateConfigurableFieldAssociationsResponse> PutViaChannelProfile(string id, UpdateConfigurableFieldAssociationsRequest request)
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
                            model.OrderConfigurableFields.Upsert(orderConfigurableField, t =>
                            {

                            });
                            continue;
                        }

                        if (model.OrderConfigurableFields.ContainsKey(orderConfigurableField))
                            model.OrderConfigurableFields.Remove(orderConfigurableField);
                    }
                    return Task.CompletedTask;
                },
            });

            return new UpdateConfigurableFieldAssociationsResponse
            {
                Data = mapper.Map<Dtos.ConfigurableFieldAssociation>(result.SelectMany(s => s.AssociatedOrderConfigurableFields!)),
            };
        }
    }
}