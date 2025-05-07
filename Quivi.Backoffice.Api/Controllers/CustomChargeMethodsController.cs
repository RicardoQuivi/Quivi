using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.CustomChargeMethods;
using Quivi.Application.Queries.CustomChargeMethods;
using Quivi.Backoffice.Api.Requests.CustomChargeMethods;
using Quivi.Backoffice.Api.Responses.CustomChargeMethods;
using Quivi.Backoffice.Api.Validations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class CustomChargeMethodsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public CustomChargeMethodsController(IQueryProcessor queryProcessor,
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
        public async Task<GetCustomChargeMethodsResponse> Get([FromQuery] GetCustomChargeMethodsRequest request)
        {
            request ??= new();
            var query = await queryProcessor.Execute(new GetCustomChargeMethodsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetCustomChargeMethodsResponse
            {
                Data = mapper.Map<Dtos.CustomChargeMethod>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateCustomChargeMethodResponse> Create([FromBody] CreateCustomChargeMethodRequest request)
        {
            using var validator = new ModelStateValidator<CreateCustomChargeMethodRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new AddCustomChargeMethodAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                LogoUrl = request.LogoUrl,
                OnInvalidName = () => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = () => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });
            if (result == null)
                throw validator.Exception;

            return new CreateCustomChargeMethodResponse
            {
                Data = mapper.Map<Dtos.CustomChargeMethod>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchCustomChargeMethodResponse> Patch(string id, [FromBody] PatchCustomChargeMethodRequest request)
        {
            using var validator = new ModelStateValidator<PatchCustomChargeMethodRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdateCustomChargeMethodsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetCustomChargeMethodsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
                UpdateAction = local =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        local.Name = request.Name;

                    if(request.LogoUrl.IsSet)
                        local.LogoUrl = request.LogoUrl;

                    return Task.CompletedTask;
                },
                OnInvalidName = local => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = local => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });

            return new PatchCustomChargeMethodResponse
            {
                Data = mapper.Map<Dtos.CustomChargeMethod?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteCustomChargeMethodResponse> Delete(string id)
        {
            await commandProcessor.Execute(new DeleteCustomChargeMethodsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetCustomChargeMethodsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                },
            });
            return new DeleteCustomChargeMethodResponse();
        }
    }
}