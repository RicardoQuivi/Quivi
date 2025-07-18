using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Queries.Reviews;
using Quivi.Backoffice.Api.Requests.Reviews;
using Quivi.Backoffice.Api.Responses.Reviews;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ReviewsController(IQueryProcessor queryProcessor,
                                        IMapper mapper,
                                        IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        [HttpGet]
        public async Task<GetReviewsResponse> Get([FromQuery] GetReviewsRequest request)
        {
            request ??= new();

            var query = await queryProcessor.Execute(new GetReviewsAsyncQuery
            {
                PosChargeIds = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetReviewsResponse
            {
                Data = mapper.Map<Dtos.Review>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }
    }
}