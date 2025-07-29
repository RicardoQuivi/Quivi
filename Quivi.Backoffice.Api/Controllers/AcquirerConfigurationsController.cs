using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.MerchantAcquirerConfigurations;
using Quivi.Application.Queries.MerchantAcquirerConfigurations;
using Quivi.Backoffice.Api.Requests.AcquirerConfigurations;
using Quivi.Backoffice.Api.Responses.AcquirerConfigurations;
using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireSubMerchant]
    [ApiController]
    [Authorize]
    public class AcquirerConfigurationsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public AcquirerConfigurationsController(IQueryProcessor queryProcessor,
                                    ICommandProcessor commandProcessor,
                                    IMapper mapper,
                                    IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        [HttpGet]
        public async Task<GetAcquirerConfigurationsResponse> Get([FromQuery] GetAcquirerConfigurationsRequest request)
        {
            request ??= new GetAcquirerConfigurationsRequest();

            var result = await queryProcessor.Execute(new GetMerchantAcquirerConfigurationsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetAcquirerConfigurationsResponse
            {
                Data = mapper.Map<Dtos.AcquirerConfiguration>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPut("cash")]
        public async Task<UpsertAcquirerConfigurationResponse> UpsertCash([FromBody] UpsertCashAcquirerConfigurationRequest request)
        {
            var result = await commandProcessor.Execute(new UpsertMerchantAcquirerConfigurationAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                ChargeMethod = ChargeMethod.Cash,
                ChargePartner = ChargePartner.Quivi,
                UpdateAction = r =>
                {
                    if (request.IsActive.HasValue)
                        r.Inactive = request.IsActive.Value == false;

                    return Task.CompletedTask;
                }
            });

            return new UpsertAcquirerConfigurationResponse
            {
                Data = mapper.Map<Dtos.AcquirerConfiguration>(result),
            };
        }

        [HttpPut("paybyrd/{name}")]
        public async Task<UpsertAcquirerConfigurationResponse> UpsertPaybyrd(ChargeMethod name, [FromBody] UpsertPaybyrdAcquirerConfigurationRequest request)
        {
            var result = await commandProcessor.Execute(new UpsertMerchantAcquirerConfigurationAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                ChargeMethod = name,
                ChargePartner = ChargePartner.Paybyrd,
                UpdateAction = r =>
                {
                    if (string.IsNullOrWhiteSpace(request.ApiKey) == false)
                        r.ApiKey = request.ApiKey;

                    if (request.IsActive.HasValue)
                        r.Inactive = request.IsActive.Value == false;

                    return Task.CompletedTask;
                }
            });

            return new UpsertAcquirerConfigurationResponse
            {
                Data = mapper.Map<Dtos.AcquirerConfiguration>(result),
            };
        }
    }
}