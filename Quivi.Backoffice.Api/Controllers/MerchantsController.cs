using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Commands.Merchants;
using Quivi.Application.Queries.Merchants;
using Quivi.Backoffice.Api.Dtos;
using Quivi.Backoffice.Api.Requests.Merchants;
using Quivi.Backoffice.Api.Requests.ModifierGroups;
using Quivi.Backoffice.Api.Responses.Merchants;
using Quivi.Backoffice.Api.Validations;
using Quivi.Domain.Entities.Charges;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;
using Quivi.Infrastructure.Pos.Facturalusa.Models.Items;
using Quivi.Infrastructure.Validations;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        public readonly IQueryProcessor queryProcessor;
        public readonly ICommandProcessor commandProcessor;
        public readonly IIdConverter idConverter;
        public readonly IMapper mapper;

        public MerchantsController(IQueryProcessor queryProcessor,
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
        public async Task<GetMerchantsResponse> Get([FromQuery] GetMerchantsRequest request)
        {
            int? parentMerchantId = string.IsNullOrWhiteSpace(request.ParentId) ? null : idConverter.FromPublicId(request.ParentId);
            IEnumerable<int>? ids = null;
            if (User.IsAdmin())
                ids = request.Ids?.Select(idConverter.FromPublicId);
            else
            {
                if (parentMerchantId.HasValue)
                    ids = request.Ids?.Select(idConverter.FromPublicId);
                else if (request.Ids?.Any() == true && request.Ids.Count() != 1 && request.Ids.SingleOrDefault() != User.SubMerchantId())
                    throw new UnauthorizedAccessException();
            }

            var merchantsQuery = await queryProcessor.Execute(new GetMerchantsAsyncQuery
            {
                Search = request.Search,
                ApplicationUserIds = User.IsAdmin() ? null : [ User.UserId(idConverter) ],
                IsDeleted = User.IsAdmin() ? null : false,
                ParentIds = parentMerchantId.HasValue ? [ parentMerchantId.Value ] : null,
                Ids = ids,
                IncludeFees = User.IsAdmin(),

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetMerchantsResponse
            {
                Data = mapper.Map<Merchant>(merchantsQuery),
                Page = merchantsQuery.CurrentPage,
                TotalPages = merchantsQuery.NumberOfPages,
                TotalItems = merchantsQuery.TotalItems,
            };
        }

        [HttpPost]
        public async Task<CreateMerchantResponse> Post([FromBody] CreateMerchantRequest request)
        {
            using var validator = new ModelStateValidator<CreateMerchantRequest, ValidationError>(request);
            var merchant = await commandProcessor.Execute(new CreateMerchantAsyncCommand
            {
                UserId = User.UserId(idConverter),

                FiscalName = request.FiscalName,
                Name = request.Name,
                PostalCode = request.PostalCode,
                VatNumber = request.VatNumber,
                Iban = request.Iban,
                LogoUrl = request.LogoUrl,
                IbanProofUrl = request.IbanProofUrl,

                OnInvalidMerchantName = () => validator.AddError(m => m.Name, ValidationError.InvalidValue),
                OnInvalidVatNumber = () => validator.AddError(m => m.VatNumber, ValidationError.InvalidValue),
                OnInvalidIban = () => validator.AddError(m => m.Iban, ValidationError.InvalidValue),
                OnVatNumberAlreadyExists = () => validator.AddError(m => m.VatNumber, ValidationError.Duplicate),
            });

            return new CreateMerchantResponse
            {
                Data = mapper.Map<Merchant>(merchant),
            };
        }

        [HttpPatch("{id}")]
        public async Task<PatchMerchantResponse> Patch(string id, [FromBody] PatchMerchantRequest request)
        {
            using var validator = new ModelStateValidator<PatchMerchantRequest, ValidationError>(request);
            var merchant = await commandProcessor.Execute(new UpdateMerchantAsyncCommand
            {
                Criteria = new Infrastructure.Abstractions.Repositories.Criterias.GetMerchantsCriteria
                {
                    Ids = [ idConverter.FromPublicId(id) ],
                    ApplicationUserIds = User.IsAdmin() ? null : [ User.UserId(idConverter) ],
                    IsDeleted = false,
                    PageSize = 1,
                },
                UpdateAction = e =>
                {
                    if (string.IsNullOrWhiteSpace(request.Name) == false)
                        e.Name = request.Name;

                    if (string.IsNullOrWhiteSpace(request.Iban) == false)
                        e.Iban = request.Iban;

                    if (string.IsNullOrWhiteSpace(request.VatNumber) == false)
                        e.VatNumber = request.VatNumber;

                    if (request.VatRate.HasValue)
                        e.VatRate = request.VatRate.Value;

                    if (string.IsNullOrWhiteSpace(request.PostalCode) == false)
                        e.PostalCode = request.PostalCode;

                    if (string.IsNullOrWhiteSpace(request.LogoUrl) == false)
                        e.LogoUrl = request.LogoUrl;

                    if (User.IsAdmin())
                    {
                        if (request.TransactionFee.HasValue)
                            e.TransactionFee = request.TransactionFee.Value;

                        if (request.TransactionFeeUnit.HasValue)
                            e.TransactionFeeUnit = request.TransactionFeeUnit.Value;

                        if (request.SurchargeFee.HasValue)
                            e.SurchargeFee = request.SurchargeFee.Value;

                        if (request.SurchargeFeeUnit.HasValue)
                            e.SurchargeFeeUnit = request.SurchargeFeeUnit.Value;

                        if (request.Inactive.HasValue)
                            e.IsDeleted = request.Inactive.Value;

                        if (request.IsDemo.HasValue)
                            e.IsDemo = request.IsDemo.Value;

                        if(request.SurchargeFees != null)
                        {
                            var fees = request.SurchargeFees.ToDictionary(s => s.Key, s => s.Value);
                            foreach (var c in e.Surcharges.ToList())
                            {
                                if (fees.ContainsKey(c.Method))
                                    continue;

                                e.Surcharges.Remove(c.Method);
                            }

                            foreach (var (method, upsertingItem) in fees)
                            {
                                e.Surcharges.Upsert(method, t =>
                                {
                                    if (upsertingItem.Fee.HasValue)
                                        t.Fee = upsertingItem.Fee.Value;

                                    if (upsertingItem.Unit.HasValue)
                                        t.Unit = upsertingItem.Unit.Value; 
                                });

                                var newValue = e.Surcharges[method];
                                if (newValue.Fee == e.SurchargeFee && newValue.Unit == e.SurchargeFeeUnit)
                                    e.Surcharges.Remove(method);
                            }
                        }
                    }

                    if (e.TermsAndConditionsAccepted == false && request.AcceptTermsAndConditions == true)
                        e.TermsAndConditionsAccepted = true;

                    return Task.CompletedTask;
                }
            });

            return new PatchMerchantResponse
            {
                Data = mapper.Map<Merchant>(merchant.SingleOrDefault()),
            };
        }
    }
}
