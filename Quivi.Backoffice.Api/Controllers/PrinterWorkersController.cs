using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PrinterWorkers;
using Quivi.Application.Queries.PrinterWorkers;
using Quivi.Backoffice.Api.Requests.PrinterWorkers;
using Quivi.Backoffice.Api.Responses.PrinterWorkers;
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
    [Authorize]
    [RequireSubMerchant]
    [ApiController]
    public class PrinterWorkersController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public PrinterWorkersController(IQueryProcessor queryProcessor,
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
        public async Task<GetPrinterWorkersResponse> Get([FromQuery] GetPrinterWorkersRequest request)
        {
            request ??= new();

            var result = await queryProcessor.Execute(new GetPrinterWorkersAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPrinterWorkersResponse
            {
                Data = mapper.Map<Dtos.PrinterWorker>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreatePrinterWorkerResponse> Get([FromBody] CreatePrinterWorkerRequest request)
        {
            using var validator = new ModelStateValidator<CreatePrinterWorkerRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new CreatePrinterWorkerAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Identifier = request.Identifier,
                Name = request.Name,
                OnDuplicateIdentifier = () => validator.AddError(p => p.Identifier, ValidationError.Duplicate),
                OnInvalidIdentifier = () => validator.AddError(p => p.Identifier, ValidationError.InvalidValue),
            });
            if (result == null)
                throw validator.Exception;

            return new CreatePrinterWorkerResponse
            {
                Data = mapper.Map<Dtos.PrinterWorker>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchPrinterWorkerResponse> Patch(string id, [FromBody] PatchPrinterWorkerRequest request)
        {
            using var validator = new ModelStateValidator<PatchPrinterWorkerRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdatePrinterWorkersAsyncCommand
            {
                Criteria = new GetPrinterWorkersCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageIndex = 0,
                    PageSize = 1,
                },
                UpdateAction = entity =>
                {
                    if(request.Name.IsSet)
                        entity.Name = request.Name;

                    return Task.CompletedTask;
                }
            });
            if (result == null)
                throw validator.Exception;

            return new PatchPrinterWorkerResponse
            {
                Data = mapper.Map<Dtos.PrinterWorker>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeletePrinterWorkerResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new UpdatePrinterWorkersAsyncCommand
            {
                Criteria = new GetPrinterWorkersCriteria
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
            });

            return new DeletePrinterWorkerResponse();
        }
    }
}