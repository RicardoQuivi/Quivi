using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PosIntegrations;
using Quivi.Application.Queries.PosIntegrations;
using Quivi.Backoffice.Api.Requests.PosIntegrations;
using Quivi.Backoffice.Api.Responses.PosIntegrations;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Repositories.Criterias;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireSubMerchant]
    [Authorize]
    public class PosIntegrationsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public PosIntegrationsController(IQueryProcessor queryProcessor,
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
        public async Task<GetPosIntegrationsResponse> Get([FromQuery] GetPosIntegrationsRequest request)
        {
            request ??= new GetPosIntegrationsRequest();

            var result = await queryProcessor.Execute(new GetPosIntegrationsAsyncQuery
            {
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                ChannelIds = string.IsNullOrWhiteSpace(request.ChannelId) ? null : [idConverter.FromPublicId(request.ChannelId)],
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetPosIntegrationsResponse
            {
                Data = mapper.Map<Dtos.PosIntegration>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }


        [HttpPost("QuiviViaFacturalusa")]
        public async Task<CreatePosIntegrationResponse> Create([FromBody] CreateQuiviViaFacturalusaPosIntegrationRequest request)
        {
            var result = await commandProcessor.Execute(new AddPosIntegrationAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                IntegrationType = IntegrationType.QuiviViaFacturalusa,
                ConnectionString = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    AccessToken = request.AccessToken,
                    SkipInvoice = request.SkipInvoice ? "1" : "0",
                    InvoicePrefix = request.InvoicePrefix,
                    IncludeTipInInvoice = request.IncludeTipInInvoice ? "1" : "0",
                }),
                DiagnosticErrorsMuted = false,
            });

            return new CreatePosIntegrationResponse
            {
                Data = mapper.Map<Dtos.PosIntegration>(result),
            };
        }

        [HttpPut("{id}/QuiviViaFacturalusa")]
        public async Task<CreatePosIntegrationResponse> Patch(string id, [FromBody] PutQuiviViaFacturalusaPosIntegrationRequest request)
        {
            var result = await commandProcessor.Execute(new UpdatePosIntegrationsAsyncCommand
            {
                Criteria = new GetPosIntegrationsCriteria
                {
                    MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],
                    PageSize = 1,
                },
                UpdateAction = integration =>
                {
                    integration.ConnectionString = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        AccessToken = request.AccessToken,
                        SkipInvoice = request.SkipInvoice ? "1" : "0",
                        InvoicePrefix = request.InvoicePrefix,
                        IncludeTipInInvoice = request.IncludeTipInInvoice ? "1" : "0",
                    });
                    return Task.CompletedTask;
                },
            });

            return new CreatePosIntegrationResponse
            {
                Data = mapper.Map<Dtos.PosIntegration>(result.SingleOrDefault()),
            };
        }
    }
}