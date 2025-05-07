using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PosCharges;
using Quivi.Application.Queries.PosCharges;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;
using Quivi.Pos.Api.Dtos.Requests.Transactions;
using Quivi.Pos.Api.Dtos.Responses.Transactions;
using Quivi.Pos.Api.Validations;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [Authorize]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public TransactionsController(IQueryProcessor queryProcessor,
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
        public async Task<GetTransactionsResponse> Get([FromQuery] GetTransactionsRequest request)
        {
            request ??= new();
            var result = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                IncludePosChargeInvoiceItems = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetTransactionsResponse
            {
                Data = mapper.Map<Dtos.Transaction>(result),
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateTransactionResponse> Create([FromBody] CreateTransactionRequest request)
        {
            using var validator = new ModelStateValidator<CreateTransactionRequest, ValidationError>(request);
            var result = await commandProcessor.Execute(new CreatePosChargeAsyncCommand
            {
                MerchantId = User.SubMerchantId(idConverter)!.Value,
                ChannelId = idConverter.FromPublicId(request.ChannelId),
                CustomChargeMethodId = idConverter.FromPublicId(request.CustomChargeMethodId),
                LocationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId),
                Email = request.Email,
                VatNumber = request.VatNumber,
                Observations = request.Observations,
                Total = request.Total,
                Tip = request.Tip,
                Items = request.Items?.Select(s => new Application.Pos.Items.SessionItem
                {
                    MenuItemId = idConverter.FromPublicId(s.MenuItemId),
                    Discount = s.DiscountPercentage,
                    Price = s.OriginalPrice,
                    Quantity = s.Quantity,
                    Extras = s.Extras.Select(e => new Application.Pos.Items.BaseSessionItem
                    {
                        MenuItemId = idConverter.FromPublicId(e.MenuItemId),
                        Price = e.OriginalPrice,
                        Quantity = e.Quantity,
                    }),
                }),

                OnIntegrationDoesNotAllowPayments = () => validator.AddError(m => m.ChannelId, ValidationError.InvalidValue),
                OnNoActiveSession = () => validator.AddError(m => m.ChannelId, ValidationError.InvalidValue),
                OnInvalidItems = () => validator.AddError(m => m.Items, ValidationError.InvalidValue),
            });
            if (result == null)
                throw validator.Exception;

            return new CreateTransactionResponse
            {
                Data = mapper.Map<Dtos.Transaction>(result),
            };
        }
    }
}