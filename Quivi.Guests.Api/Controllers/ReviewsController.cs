using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Reviews;
using Quivi.Application.Queries.Reviews;
using Quivi.Guests.Api.Dtos.Requests.Reviews;
using Quivi.Guests.Api.Dtos.Responses.Reviews;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public ReviewsController(IQueryProcessor queryProcessor,
                                       IMapper mapper,
                                       IIdConverter idConverter,
                                       ICommandProcessor commandProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.commandProcessor = commandProcessor;
        }

        [HttpGet("{id}")]
        public async Task<GetReviewResponse> Get(string id)
        {
            var query = await queryProcessor.Execute(new GetReviewsAsyncQuery
            {
                PosChargeIds = [idConverter.FromPublicId(id)],
                PageIndex = 0,
                PageSize = 1,
            });

            return new GetReviewResponse
            {
                Data = mapper.Map<Dtos.Review>(query.SingleOrDefault()),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchReviewResponse> Put(string id, [FromBody] PatchReviewRequest request)
        {
            var result = await commandProcessor.Execute(new UpsertReviewAsyncCommand
            {
                PosChargeId = idConverter.FromPublicId(id),
                UpdateAction = r =>
                {
                    if(request.Stars.HasValue)
                        r.Stars = request.Stars.Value;

                    if(request.Comment.IsSet)
                        r.Comment = request.Comment.Value;

                    return Task.CompletedTask;
                },
            });

            return new PatchReviewResponse
            {
                Data = mapper.Map<Dtos.Review>(result),
            };
        }
    }
}