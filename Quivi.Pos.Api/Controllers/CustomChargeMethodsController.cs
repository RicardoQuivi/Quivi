using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.CustomChargeMethods;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.CustomChargeMethods;
using Quivi.Pos.Api.Dtos.Responses.CustomChargeMethods;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
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
            var result = await queryProcessor.Execute(new GetCustomChargeMethodsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetCustomChargeMethodsResponse
            {
                Data = mapper.Map<Dtos.CustomChargeMethod>(result),
                Page = result.CurrentPage,
                TotalItems = result.TotalItems,
                TotalPages = result.NumberOfPages,
            };
        }
    }
}
