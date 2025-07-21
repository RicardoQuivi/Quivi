using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.PrinterMessageTargets;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Printers;
using Quivi.Pos.Api.Dtos.Responses.Printers;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [RequireEmployee]
    [ApiController]
    public class PrintersController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public PrintersController(IQueryProcessor queryProcessor,
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
        public async Task<GetPrintersResponse> Get([FromQuery] GetPrintersRequest request)
        {
            var result = await queryProcessor.Execute(new GetPrinterMessageTargetsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                IncludePrinterNotificationsContactBaseNotificationsContact = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPrintersResponse
            {
                Data = mapper.Map<Dtos.Printer>(result),
                Page = result.CurrentPage,
                TotalItems = result.TotalItems,
                TotalPages = result.NumberOfPages,
            };
        }
    }
}