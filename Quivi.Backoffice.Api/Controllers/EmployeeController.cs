using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.Employees;
using Quivi.Application.Queries.Employees;
using Quivi.Backoffice.Api.Requests.Employees;
using Quivi.Backoffice.Api.Responses.Employees;
using Quivi.Backoffice.Api.Validations;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [RequireSubMerchant]
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
            request ??= new();
            var query = await queryProcessor.Execute(new GetEmployeesAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                IsDeleted = false,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetEmployeesResponse
            {
                Data = mapper.Map<Dtos.Employee>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateEmployeeResponse> Create([FromBody] CreateEmployeeRequest request)
        {
            using var validator = new ModelStateValidator<CreateEmployeeRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new AddEmployeeAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                Name = request.Name,

                OnInvalidName = () => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = () => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });
            if (result == null)
                throw validator.Exception;

            return new CreateEmployeeResponse
            {
                Data = mapper.Map<Dtos.Employee>(result),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchEmployeeResponse> Patch(string id, [FromBody] PatchEmployeeRequest request)
        {
            using var validator = new ModelStateValidator<PatchEmployeeRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new UpdateEmployeesAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetEmployeesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    IsDeleted = false,
                },
                UpdateAction = employee =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        employee.Name = request.Name;

                    if(request.InactivityLogoutTimeout.IsSet)
                        employee.InactivityLogoutTimeout = request.InactivityLogoutTimeout;

                    if(request.Restrictions != null)
                        employee.Restrictions = mapper.Map<IEnumerable<Dtos.EmployeeRestriction>, EmployeeRestrictions>(request.Restrictions);

                    return Task.CompletedTask;
                },
                OnInvalidName = local => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnNameAlreadyExists = local => validator.AddError(m => m.Name, ValidationError.Duplicate),
            });

            return new PatchEmployeeResponse
            {
                Data = mapper.Map<Dtos.Employee?>(result.SingleOrDefault()),
            };
        }

        [HttpDelete("{id}")]
        public async Task<DeleteEmployeeResponse> Delete(string id)
        {
            var result = await commandProcessor.Execute(new UpdateEmployeesAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetEmployeesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    IsDeleted = false,
                },
                UpdateAction = employee =>
                {
                    employee.IsDeleted = true;
                    return Task.CompletedTask;
                },
                OnInvalidName = local => { },
                OnNameAlreadyExists = local => { },
            });

            return new DeleteEmployeeResponse();
        }

        [HttpDelete]
        [Route("{id}/pincode")]
        public async Task<DeleteEmployeePinCodeResponse> ResetPinCode(string id)
        {
            var result = await commandProcessor.Execute(new UpdateEmployeesAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetEmployeesCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    IsDeleted = false,
                },
                UpdateAction = employee =>
                {
                    employee.PinCode = null;
                    return Task.CompletedTask;
                },
                OnInvalidName = local => { },
                OnNameAlreadyExists = local => { },
            });

            return new DeleteEmployeePinCodeResponse
            {
                Data = mapper.Map<Dtos.Employee?>(result.SingleOrDefault()),
            };
        }
    }
}
