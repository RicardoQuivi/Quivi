using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Locations;
using Quivi.Application.Queries.Locations;
using Quivi.Backoffice.Api.Requests.Locals;
using Quivi.Backoffice.Api.Responses.Locals;
using Quivi.Backoffice.Api.Validations;
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
    public class LocalsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public LocalsController(IQueryProcessor queryProcessor,
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
        public async Task<GetLocalsResponse> Get([FromQuery] GetLocalsRequest request)
        {
            request ??= new();
            var query = await queryProcessor.Execute(new GetLocationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                IsDeleted = false,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetLocalsResponse
            {
                Data = mapper.Map<Dtos.Local>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateLocalResponse> Create([FromBody] CreateLocalRequest request)
        {
            using var validator = new ModelStateValidator<CreateLocalRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new AddLocationAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                OnInvalidName = () => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = () => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });
            if (result == null)
                throw validator.Exception;

            return new CreateLocalResponse
            {
                Data = mapper.Map<Dtos.Local>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchLocalResponse> Patch(string id, [FromBody] PatchLocalRequest request)
        {
            using var validator = new ModelStateValidator<PatchLocalRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdateLocationsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetLocationsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    IsDeleted = false,
                },
                UpdateAction = local =>
                {
                    if(string.IsNullOrWhiteSpace(request.Name) == false)
                        local.Name = request.Name;

                    return Task.CompletedTask;
                },
                OnInvalidName = local => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = local => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });

            return new PatchLocalResponse
            {
                Data = mapper.Map<Dtos.Local?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteLocalResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new UpdateLocationsAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetLocationsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    IsDeleted = false,
                },
                UpdateAction = local =>
                {
                    local.IsDeleted = true;
                    return Task.CompletedTask;
                },
                OnInvalidName = local => { },
                OnNameAlreadyExists = local => { },
            });

            return new DeleteLocalResponse();
        }
    }
}
