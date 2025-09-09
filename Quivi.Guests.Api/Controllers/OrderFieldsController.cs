using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.OrderConfigurableFields;
using Quivi.Guests.Api.Dtos;
using Quivi.Guests.Api.Dtos.Requests.OrderFields;
using Quivi.Guests.Api.Dtos.Responses.OrderFields;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderFieldsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public OrderFieldsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<GetOrderFieldsResponse> Get([FromQuery] GetOrderFieldsRequest request)
        {
            var query = await queryProcessor.Execute(new GetOrderConfigurableFieldsAsyncQuery
            {
                ChannelIds = [idConverter.FromPublicId(request.ChannelId)],
                IncludeTranslations = true,
                ForOrdering = true,
                IsAutoFill = false,
                IsDeleted = false,

                PageSize = null,
                PageIndex = 0,
            });

            return new GetOrderFieldsResponse
            {
                Data = mapper.Map<OrderField>(query),
            };
        }
    }
}