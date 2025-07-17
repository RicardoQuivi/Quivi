using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.Merchants;
using Quivi.Guests.Api.Dtos.Responses.Merchants;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public MerchantsController(IIdConverter idConverter, IQueryProcessor queryProcessor, IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<GetMerchantResponse> Get(string id)
        {
            var result = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Ids = [idConverter.FromPublicId(id)],
                IncludeFees = true,
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetMerchantResponse
            {
                Data = mapper.Map<Dtos.Merchant>(result.FirstOrDefault()),
            };
        }
    }
}