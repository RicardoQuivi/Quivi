using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Guests.Api.Dtos.Requests.PaymentMethods;
using Quivi.Guests.Api.Dtos.Responses.PaymentMethods;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public PaymentMethodsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("{channelId}")]
        public async Task<GetPaymentMethodsResponse> Get(string channelId, [FromQuery] GetPaymentMethodsRequest request)
        {
            var result = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                ChannelIds = [idConverter.FromPublicId(channelId)],
                IsDeleted = false,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPaymentMethodsResponse
            {
                Data = mapper.Map<Dtos.PaymentMethod>(result),
                Page = result.CurrentPage,
                TotalItems = result.TotalItems,
                TotalPages = result.NumberOfPages,
            };
        }
    }
}
