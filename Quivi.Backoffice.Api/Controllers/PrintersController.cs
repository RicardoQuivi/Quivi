using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PrinterNotificationsContacts;
using Quivi.Application.Queries.PrinterNotificationsContacts;
using Quivi.Backoffice.Api.Requests.Printers;
using Quivi.Backoffice.Api.Responses.Printers;
using Quivi.Backoffice.Api.Validations;
using Quivi.Domain.Entities.Notifications;
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
    public class PrintersController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public PrintersController(IQueryProcessor queryProcessor,
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
        public async Task<GetPrintersResponse> Get([FromQuery] GetPrintersRequest request)
        {
            request ??= new();

            var result = await queryProcessor.Execute(new GetPrinterNotificationsContactsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PrinterWorkerIds = string.IsNullOrWhiteSpace(request.PrinterWorkerId) ? null : [idConverter.FromPublicId(request.PrinterWorkerId)],
                IncludeNotificationsContact = true,
                IsDeleted = false,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPrintersResponse
            {
                Data = mapper.Map<Dtos.Printer>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreatePrinterResponse> Create([FromBody] CreatePrinterRequest request)
        {
            using var validator = new ModelStateValidator<CreatePrinterRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new CreatePrinterNotificationsContactAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,
                Address = request.Address,
                PrinterWorkerId = idConverter.FromPublicId(request.PrinterWorkerId),
                LocationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId),
                Notifications = mapper.Map<IEnumerable<Dtos.NotificationType>, NotificationMessageType>(request.Notifications),
                OnInvalidName = () => validator.AddError(p => p.Name, ValidationError.InvalidValue),
                OnInvalidAddress = () => validator.AddError(p => p.Address, ValidationError.InvalidValue),
            });
            if (result == null)
                throw validator.Exception;

            return new CreatePrinterResponse
            {
                Data = mapper.Map<Dtos.Printer>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchPrinterResponse> Patch(string id, [FromBody] PatchPrinterRequest request)
        {
            using var validator = new ModelStateValidator<PatchPrinterRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdatePrinterNotificationsContactsAsyncCommand
            {
                Criteria = new GetPrinterNotificationsContactsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageIndex = 0,
                    PageSize = 1,
                },
                UpdateAction = entity =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        entity.Name = request.Name;

                    if (string.IsNullOrWhiteSpace(request.Address) == false)
                        entity.Address = request.Address;

                    if (string.IsNullOrWhiteSpace(request.PrinterWorkerId) == false)
                        entity.PrinterWorkerId = idConverter.FromPublicId(request.PrinterWorkerId);

                    if (request.LocationId.IsSet)
                        entity.LocationId = string.IsNullOrWhiteSpace(request.LocationId.Value) ? null : idConverter.FromPublicId(request.LocationId.Value);

                    if (request.Notifications != null)
                        entity.Notifications = mapper.Map<IEnumerable<Dtos.NotificationType>, NotificationMessageType>(request.Notifications);

                    return Task.CompletedTask;
                }
            });
            if (result == null)
                throw validator.Exception;

            return new PatchPrinterResponse
            {
                Data = mapper.Map<Dtos.Printer>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeletePrinterResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new UpdatePrinterNotificationsContactsAsyncCommand
            {
                Criteria = new GetPrinterNotificationsContactsCriteria
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

            return new DeletePrinterResponse();
        }
    }
}