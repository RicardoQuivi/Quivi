using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Backoffice.Api.Responses.PublicIds;
using Quivi.Infrastructure.Abstractions.Converters;

namespace Quivi.Backoffice.Api.Dtos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PublicIdsController : ControllerBase
    {
        private readonly IIdConverter idConverter;

        public PublicIdsController(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        [HttpGet("{id}")]
        public GetPublicIdResponse Get(string id) => new GetPublicIdResponse
        {
            Data = idConverter.FromPublicId(id),
        };
    }
}