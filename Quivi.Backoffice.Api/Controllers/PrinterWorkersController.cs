using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.PrinterWorkers;
using Quivi.Backoffice.Api.Requests.PrinterWorkers;
using Quivi.Backoffice.Api.Responses.PrinterWorkers;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

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
    }
}