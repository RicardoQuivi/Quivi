using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.Reports;
using Quivi.Backoffice.Api.Requests.Reporting;
using Quivi.Backoffice.Api.Responses.Reporting;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Extensions;

namespace Quivi.Backoffice.Api.Controllers
{
    [Route("api/[controller]")]
    [RequireMerchant]
    [ApiController]
    public class ReportingController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IIdConverter idConverter;
        private readonly IMapper mapper;

        public ReportingController(IIdConverter idConverter,
                                        ICommandProcessor commandProcessor,
                                        IQueryProcessor queryProcessor,
                                        IMapper mapper)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
        }

        [HttpGet("sales")]
        public async Task<GetSalesResponse> GetSales([FromQuery] GetSalesRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var query = await queryProcessor.Execute(new GetSalesAsyncQuery
            {
                Period = request.Period,
                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],

                From = request.From?.UtcDateTime,
                To = request.To?.UtcDateTime,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetSalesResponse
            {
                Data = mapper.Map<Dtos.Sales>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }

        [HttpGet("sales/products")]
        public async Task<GetProductSalesResponse> GetProductSales([FromQuery] GetProductSalesRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var query = await queryProcessor.Execute(new GetProductSalesAsyncQuery
            {
                Period = request.Period,
                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],

                From = request.From?.UtcDateTime,
                To = request.To?.UtcDateTime,
                SortBy = request.SortBy,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetProductSalesResponse
            {
                Data = mapper.Map<Dtos.ProductSales>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }

        [HttpGet("sales/categories")]
        public async Task<GetCategorySalesResponse> GetCategoriesSales([FromQuery] GetCategorySalesRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var query = await queryProcessor.Execute(new GetCategorySalesAsyncQuery
            {
                Period = request.Period,
                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],

                From = request.From?.UtcDateTime,
                To = request.To?.UtcDateTime,
                SortBy = request.SortBy,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetCategorySalesResponse
            {
                Data = mapper.Map<Dtos.CategorySales>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }

        [HttpGet("sales/chargemethods")]
        public async Task<GetChargeMethodSalesResponse> GetChargeMethodSales([FromQuery] GetChargeMethodSalesRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var query = await queryProcessor.Execute(new GetChargeMethodSalesAsyncQuery
            {
                Period = request.Period,
                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],

                From = request.From?.UtcDateTime,
                To = request.To?.UtcDateTime,
                SortBy = request.SortBy,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetChargeMethodSalesResponse
            {
                Data = mapper.Map<Dtos.ChargeMethodSales>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }

        [HttpGet("sales/chargemethods/quivi")]
        public async Task<GetPartnerChargeMethodSalesResponse> GetQuiviChargeMethodSales([FromQuery] GetPartnerChargeMethodSalesRequest request)
        {
            request ??= new();

            bool adminView = request.AdminView == true && (User.IsAdmin() || User.IsSuperAdmin());
            var merchantId = User.MerchantId(idConverter);
            var subMerchantId = User.SubMerchantId(idConverter);

            var query = await queryProcessor.Execute(new GetPartnerChargeMethodSalesAsyncQuery
            {
                Period = request.Period,

                ChargePartners = request.ChargePartners,
                ChargeMethods = request.ChargeMethods,

                ParentMerchantIds = adminView ? null : [merchantId!.Value],
                MerchantIds = adminView || subMerchantId.HasValue == false ? null : [subMerchantId.Value],

                From = request.From?.UtcDateTime,
                To = request.To?.UtcDateTime,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });
            return new GetPartnerChargeMethodSalesResponse
            {
                Data = mapper.Map<Dtos.PartnerChargeMethodSales>(query),
                Page = query.CurrentPage,
                TotalItems = query.TotalItems,
                TotalPages = query.NumberOfPages,
            };
        }
    }
}