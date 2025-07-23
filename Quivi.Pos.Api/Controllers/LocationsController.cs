using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.Locations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Responses.Locations;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public LocationsController(IQueryProcessor queryProcessor,
                            ICommandProcessor commandProcessor,
                            IIdConverter idConverter,
                            IMapper mapper)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
        }

        public async Task<GetLocationsResponse> Get()
        {
            var query = await queryProcessor.Execute(new GetLocationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PageSize = null,
            });

            return new GetLocationsResponse
            {
                Data = mapper.Map<Dtos.Location>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }
    }
}