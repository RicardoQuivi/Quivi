using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Charges;
using Quivi.Application.Commands.PosCharges;
using Quivi.Application.Queries.PosCharges;
using Quivi.Domain.Entities.Charges;
using Quivi.Guests.Api.Dtos.Requests.Transactions;
using Quivi.Guests.Api.Dtos.Responses.Transactions;
using Quivi.Guests.Api.Validations;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Validations;

namespace Quivi.Guests.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public TransactionsController(IQueryProcessor queryProcessor,
                                       IMapper mapper,
                                       IIdConverter idConverter,
                                       ICommandProcessor commandProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.commandProcessor = commandProcessor;
        }

        [HttpGet]
        public async Task<GetTransactionsResponse> Get([FromQuery] GetTransactionsRequest request)
        {
            request = request ?? new GetTransactionsRequest();
            if (string.IsNullOrWhiteSpace(request.SessionId) && string.IsNullOrWhiteSpace(request.Id) && string.IsNullOrWhiteSpace(request.OrderId))
                return new GetTransactionsResponse
                {
                    Data = [],
                    Page = 0,
                    TotalItems = 0,
                    TotalPages = 0,
                };

            var query = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = string.IsNullOrWhiteSpace(request.Id) ? null : [idConverter.FromPublicId(request.Id)],
                SessionIds = string.IsNullOrWhiteSpace(request.SessionId) ? null : [idConverter.FromPublicId(request.SessionId)],
                OrderIds = string.IsNullOrWhiteSpace(request.OrderId) ? null : [idConverter.FromPublicId(request.OrderId)],
                IsCaptured = string.IsNullOrWhiteSpace(request.SessionId) == false || string.IsNullOrWhiteSpace(request.OrderId) == false ? true : null,
                IncludePosChargeSyncAttempts = true,
                IncludeCharge = true,
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetTransactionsResponse
            {
                Data = mapper.Map<Dtos.Transaction>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }

        [HttpPost]
        public async Task<CreateTransactionResponse> Create([FromBody] CreateTransactionRequest request)
        {
            var language = Request.Headers.AcceptLanguage.ToString();

            using var validator = new ModelStateValidator<CreateTransactionRequest, ValidationError>(request);
            var posCharge = await commandProcessor.Execute(new CreateGuestPosChargeAsyncCommand
            {
                ChannelId = idConverter.FromPublicId(request.ChannelId),
                MerchantAcquirerConfigurationId = idConverter.FromPublicId(request.MerchantAcquirerConfigurationId),
                ConsumerPersonId = string.IsNullOrWhiteSpace(request.ConsumerPersonId) ? null : idConverter.FromPublicId(request.ConsumerPersonId),
                Tip = request.Tip,
                Amount = request.Amount,
                Email = request.Email,
                VatNumber = request.VatNumber,
                SurchargeFeeOverride = null,
                PayAtTheTableData = request.PayAtTheTableData != null ? new CreateGuestPosChargeAsyncCommand.PayAtTheTable
                {
                    Items = request.PayAtTheTableData.Items?.Select(s => new Application.Pos.Items.SessionItem
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
                } : null,
                OrderAndPayData = request.OrderAndPayData != null ? new CreateGuestPosChargeAsyncCommand.OrderAndPay
                {
                    OrderId = idConverter.FromPublicId(request.OrderAndPayData.OrderId),
                } : null,
                UserLanguageIso = string.IsNullOrWhiteSpace(language) ? "pt-PT" : language,

                OnInvalidAdditionalData = () =>
                {
                    validator.AddError(m => m.PayAtTheTableData, ValidationError.Invalid);
                    validator.AddError(m => m.OrderAndPayData, ValidationError.Invalid);
                },
                OnInvalidChannel = () => validator.AddError(m => m.ChannelId, ValidationError.Invalid),
                OnInvalidMerchantAcquirerConfiguration = () => validator.AddError(m => m.MerchantAcquirerConfigurationId, ValidationError.Invalid),
                OnInvalidAmount = () => validator.AddError(m => m.Amount, ValidationError.Invalid),
                OnInvalidTip = () => validator.AddError(m => m.Tip, ValidationError.Invalid),
                OnNoOpenSession = () => validator.AddError(m => m.ChannelId, ValidationError.Invalid),
            });
            if (posCharge == null)
                throw validator.Exception;

            if (posCharge.Charge!.ChargeMethod == ChargeMethod.CreditCard)
            {
                //TODO: Uncomment when credit card charge is implemented
                //var startCreditCardChargeResult = await _commandHandlerProcessor.Execute(new StartCreditCardChargeAsyncCommand(posCharge.Data.ChargeGuid));
                //if (startCreditCardChargeResult.Success == false) 
                //    throw startCreditCardChargeResult.Exception;
            }

            return new CreateTransactionResponse
            {
                Data = mapper.Map<Dtos.Transaction>(posCharge),
            };
        }

        [HttpPut("{id}/" + nameof(ChargeMethod.Cash))]
        public async Task<PutTransactionResponse> Cash(string id, [FromBody] PutCashTransactionRequest request)
        {
            request = request ?? new PutCashTransactionRequest();
            return await StartCharge(id, request, charge => commandProcessor.Execute(new ProcessChargeAsyncCommand
            {
                Id = charge.Id,
            }));
        }

        private async Task<PutTransactionResponse> StartCharge<T>(string id, T request, Func<Charge, Task<Charge?>> func) where T : PutTransactionRequest
        {
            var charges = await commandProcessor.Execute(new StartChargesProcessingAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetChargesCriteria
                {
                    Ids = [idConverter.FromPublicId(id)],
                },
            });
            var charge = charges.SingleOrDefault();

            if(charge == null)
                return new PutTransactionResponse
                {
                    Data = null,
                };

            charge = await func(charge);
            if (charge == null)
                return new PutTransactionResponse
                {
                    Data = null,
                };

            var query = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                Ids = [charge.Id],
                IncludePosChargeSyncAttempts = true,
                IncludeCharge = true,
                PageIndex = 0,
                PageSize = 1,
            });
            return new PutTransactionResponse
            {
                Data = mapper.Map<Dtos.Transaction>(query.SingleOrDefault()),
            };
        }
    }
}