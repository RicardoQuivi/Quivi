using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Employees;
using Quivi.Application.Queries.Employees;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Pos.Api.Dtos.Requests.Employees;
using Quivi.Pos.Api.Dtos.Responses.Employees;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public EmployeesController(IQueryProcessor queryProcessor,
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
        public async Task<GetEmployeesResponse> Get([FromQuery] GetEmployeesRequest request)
        {
            request = request ?? new GetEmployeesRequest();
            var result = await queryProcessor.Execute(new GetEmployeesAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                IsDeleted = request.IncludeDeleted ? null : false,
                PageSize = request.PageSize,
                PageIndex = request.Page,
            });

            return new GetEmployeesResponse
            {
                Data = mapper.Map<Dtos.Employee>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPut("{id}/pincode")]
        public async Task<PatchEmployeePinCodeResponse> PatchPinCode(string id, [FromBody] PatchEmployeePinCodeRequest request)
        {
            var result = await commandProcessor.Execute(new UpdateEmployeesAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetEmployeesCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    IsDeleted = false,
                },
                UpdateAction = (c) =>
                {
                    var loggedEmployeeId = User.EmployeeId(idConverter);
                    if (c.HasPinCode && (!loggedEmployeeId.HasValue || c.Id != loggedEmployeeId.Value))
                        throw new UnauthorizedAccessException();

                    c.PinCode = request.PinCode;
                    return Task.CompletedTask;
                },
                OnInvalidName = (c) => { },
                OnNameAlreadyExists = (c) => { },
            });

            return new PatchEmployeePinCodeResponse
            {
                Data = mapper.Map<Dtos.Employee?>(result.SingleOrDefault()),
            };
        }
    }
}