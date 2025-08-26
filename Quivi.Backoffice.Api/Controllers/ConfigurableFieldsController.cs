using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.OrderConfigurableFields;
using Quivi.Application.Queries.OrderConfigurableFields;
using Quivi.Backoffice.Api.Requests.ConfigurableFields;
using Quivi.Backoffice.Api.Responses.ConfigurableFields;
using Quivi.Backoffice.Api.Validations;
using Quivi.Domain.Entities;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireSubMerchant]
    public class ConfigurableFieldsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ConfigurableFieldsController(IQueryProcessor queryProcessor,
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
        public async Task<GetConfigurableFieldsResponse> Get([FromQuery] GetConfigurableFieldsRequest request)
        {
            request ??= new GetConfigurableFieldsRequest();

            var query = await queryProcessor.Execute(new GetOrderConfigurableFieldsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ChannelProfileIds = request.ChannelProfileIds?.Select(idConverter.FromPublicId),
                IncludeTranslations = true,
                IsDeleted = false,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetConfigurableFieldsResponse
            {
                Data = mapper.Map<Dtos.ConfigurableField>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateConfigurableFieldsResponse> Create([FromBody] CreateConfigurableFieldsRequest request)
        {
            using var validator = new ModelStateValidator<CreateConfigurableFieldsRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new AddOrderConfigurableFieldAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,

                Name = request.Name,
                IsRequired = request.IsRequired,
                IsAutoFill = request.IsAutoFill,
                PrintedOn = mapper.Map<PrintedOn>(request.PrintedOn),
                AssignedOn = mapper.Map<AssignedOn>(request.AssignedOn),
                FieldType = mapper.Map<FieldType>(request.Type),
                DefaultValue = request.DefaultValue,
                Translations = request.Translations?.ToDictionary(r => r.Key, r => r.Value.Name),

                OnInvalidName = () => validator.AddError(r => r.Name, ValidationError.InvalidValue),
                OnAutoFillWithEmptyDefaultValue = () => validator.AddError(r => r.DefaultValue, ValidationError.Required),
                OnInvalidDefaultValue = () => validator.AddError(r => r.DefaultValue, ValidationError.InvalidValue),
            });
            if (result == null)
                throw validator.Exception;

            return new CreateConfigurableFieldsResponse
            {
                Data = mapper.Map<Dtos.ConfigurableField>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<UpdateConfigurableFieldResponse> Patch(string id, [FromBody] UpdateConfigurableFieldRequest request)
        {
            using var validator = new ModelStateValidator<UpdateConfigurableFieldRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdateOrderConfigurableFieldsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetOrderConfigurableFieldsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
                UpdateAction = (model) =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        model.Name = request.Name;

                    if (request.IsRequired.HasValue)
                        model.IsRequired = request.IsRequired.Value;

                    if (request.IsAutoFill.HasValue)
                        model.IsAutoFill = request.IsAutoFill.Value;

                    if (request.PrintedOn.HasValue)
                        model.PrintedOn = mapper.Map<PrintedOn>(request.PrintedOn.Value);

                    if (request.AssignedOn.HasValue)
                        model.AssignedOn = mapper.Map<AssignedOn>(request.AssignedOn.Value);

                    if (request.Type.HasValue)
                        model.Type = mapper.Map<FieldType>(request.Type.Value);

                    if (request.DefaultValue.IsSet)
                        model.DefaultValue = request.DefaultValue.Value;

                    if (request.Translations != null)
                    {
                        var allRequestLanguages = request.Translations.ToDictionary(t => t.Key);
                        foreach (var languagesToDelete in Enum.GetValues(typeof(Language)).Cast<Language>().Where(r => allRequestLanguages.ContainsKey(r) == false))
                            model.Translations.Remove(languagesToDelete);

                        foreach (var t in request.Translations)
                            model.Translations.Upsert(t.Key, translation => translation.Name = t.Value.Name);
                    }

                    return Task.CompletedTask;
                },
                OnAutoFillWithEmptyDefaultValue = () => validator.AddError(r => r.DefaultValue, ValidationError.Required),
                OnInvalidDefaultValue = () => validator.AddError(r => r.DefaultValue, ValidationError.InvalidValue),
            });
            if (validator.Exception.HasErrors)
                throw validator.Exception;

            return new UpdateConfigurableFieldResponse
            {
                Data = mapper.Map<Dtos.ConfigurableField>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteConfigurableFieldResponse> Delete(string id)
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
                    model.IsDeleted = true;
                    return Task.CompletedTask;
                },
                OnAutoFillWithEmptyDefaultValue = () => { },
                OnInvalidDefaultValue = () => { },
            });
            return new DeleteConfigurableFieldResponse();
        }
    }
}