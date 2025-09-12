using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Commands.PosCharges;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Application.Queries.PosChargeInvoiceItems;
using Quivi.Application.Queries.PosCharges;
using Quivi.Application.Services.Exceptions;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Pos;
using Quivi.Infrastructure.Abstractions.Services.Charges;
using Quivi.Infrastructure.Abstractions.Services.Charges.Parameters;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Validations;
using Quivi.Pos.Api.Dtos.Requests.Transactions;
using Quivi.Pos.Api.Dtos.Responses.Transactions;
using Quivi.Pos.Api.Validations;

namespace Quivi.Pos.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireEmployee]
    [RequireSubMerchant]
    [Authorize]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly ICommandProcessor commandProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;
        private readonly IAcquirerChargeProcessor acquirerChargeProcessor;

        public TransactionsController(IQueryProcessor queryProcessor,
                                    ICommandProcessor commandProcessor,
                                    IIdConverter idConverter,
                                    IMapper mapper,
                                    IAcquirerChargeProcessor acquirerChargeProcessor)
        {
            this.queryProcessor = queryProcessor;
            this.commandProcessor = commandProcessor;
            this.idConverter = idConverter;
            this.mapper = mapper;
            this.acquirerChargeProcessor = acquirerChargeProcessor;
        }

        [HttpGet]
        public async Task<GetTransactionsResponse> Get([FromQuery] GetTransactionsRequest request)
        {
            request ??= new();
            var result = await queryProcessor.Execute(new GetPosChargesAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                OrderIds = request.OrderIds?.Select(idConverter.FromPublicId),
                IncludePosChargeInvoiceItems = true,
                IncludeMerchantCustomCharge = true,
                IsCaptured = true,

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

        [HttpGet("resume")]
        public async Task<GetTransactionsResumeResponse> GetResume([FromQuery] GetTransactionsResumeRequest request)
        {
            request ??= new();
            var result = await queryProcessor.Execute(new GetPosChargesResumeAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                Ids = request.Ids?.Select(idConverter.FromPublicId),
                IsCaptured = true,
            });

            return new GetTransactionsResumeResponse
            {
                Data = new Dtos.TransactionsResume
                {
                    Payment = result.PaymentAmount,
                    Tip = result.TipAmount,
                },
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
                EmployeeId = User.EmployeeId(idConverter)!.Value,
                LocationId = string.IsNullOrWhiteSpace(request.LocationId) ? null : idConverter.FromPublicId(request.LocationId),
                Email = request.Email,
                VatNumber = request.VatNumber,
                Observations = request.Observations,
                Amount = request.Amount,
                Tip = request.Tip,
                Items = request.Items?.Select(s => new SessionItem
                {
                    MenuItemId = idConverter.FromPublicId(s.MenuItemId),
                    Discount = s.DiscountPercentage,
                    Price = s.OriginalPrice,
                    Quantity = s.Quantity,
                    Extras = s.Extras.Select(e => new SessionExtraItem
                    {
                        ModifierGroupId = idConverter.FromPublicId(e.ModifierGroupId),
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

        [HttpGet]
        [Route("{id}/items")]
        public async Task<GetTransactionItemsResponse> GetTransactionItems(string id, [FromQuery] GetTransactionItemsRequest request)
        {
            var result = await queryProcessor.Execute(new GetPosChargeInvoiceItemsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PosChargeIds = [idConverter.FromPublicId(id)],
                IsParent = true,

                IncludeOrderMenuItem = true,
                IncludeChildrenPosChargeInvoiceItems = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetTransactionItemsResponse
            {
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
                Data = mapper.Map<Dtos.TransactionItem>(result),
            };
        }

        [HttpGet]
        [Route("{id}/documents")]
        public async Task<GetTransactionDocumentsResponse> GetDocuments(string id, [FromQuery] GetTransactionDocumentsRequest request)
        {
            var result = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PosChargeIds = [idConverter.FromPublicId(id)],
                HasDownloadPath = true,
                Formats = [DocumentFormat.Pdf],
                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetTransactionDocumentsResponse
            {
                Page = result.CurrentPage,
                TotalPages = result.NumberOfPages,
                TotalItems = result.TotalItems,
                Data = mapper.Map<Dtos.TransactionDocument>(result),
            };
        }

        [HttpPost("{id}/refund")]
        public async Task<RefundTransactionResponse> Refund(string id, [FromBody] RefundTransactionRequest request)
        {
            try
            {
                await acquirerChargeProcessor.Refund(new RefundParameters
                {
                    Amount = request.Amount,
                    ChargeId = idConverter.FromPublicId(id),
                    MerchantId = User.IsAdmin() ? null : User.SubMerchantId(idConverter)!.Value,
                    IsCancellation = request.Cancelation,
                    EmployeeId = User.EmployeeId(idConverter)!.Value,
                    Reason = request.Reason,
                });

                var result = await queryProcessor.Execute(new GetPosChargesAsyncQuery
                {
                    ParentMerchantIds = User.IsAdmin() ? null : [User.MerchantId(idConverter)!.Value],
                    MerchantIds = User.IsAdmin() ? null : [User.SubMerchantId(idConverter)!.Value],
                    Ids = [idConverter.FromPublicId(id)],

                    IncludePosChargeInvoiceItemsOrderMenuItems = true,
                    IncludeMerchantCustomCharge = true,
                    IncludePosChargeSyncAttempts = true,

                    PageIndex = 0,
                    PageSize = 1,
                });

                return new RefundTransactionResponse
                {
                    Data = mapper.Map<Dtos.Transaction>(result.SingleOrDefault()),
                };
            }
            catch (Exception ex)
            {
                if (ex is NoBalanceException e)
                {
                    using var validator = new ModelStateValidator<string, ValidationError>(id);
                    validator.AddError(e => e, ValidationError.NoBalance);
                    throw validator.Exception;
                }

                throw;
            }
        }
    }
}