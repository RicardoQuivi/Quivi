using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.PosChargeInvoiceItems;
using Quivi.Application.Queries.Reports;
using Quivi.Backoffice.Api.Requests.Reporting;
using Quivi.Backoffice.Api.Responses.Reporting;
using Quivi.Domain.Entities.Charges;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Cqrs;
using Quivi.Infrastructure.Abstractions.Mapping;
using Quivi.Infrastructure.Abstractions.Services.Exporters;
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
        private readonly IDataExporter dataExporter;

        public ReportingController(IIdConverter idConverter,
                                        ICommandProcessor commandProcessor,
                                        IQueryProcessor queryProcessor,
                                        IMapper mapper,
                                        IDataExporter dataExporter)
        {
            this.idConverter = idConverter;
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.dataExporter = dataExporter;
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

        [HttpGet("sales/export/{type}")]
        public async Task<ExportSalesResponse> ExportSales(ExportType type, [FromQuery] ExportSalesRequest request)
        {
            var subMerchantId = User.SubMerchantId(idConverter);
            var preResult = await queryProcessor.Execute(new GetPosChargeInvoiceItemsAsyncQuery
            {
                ParentMerchantIds = [User.MerchantId(idConverter)!.Value],
                MerchantIds = subMerchantId.HasValue == false ? null : [subMerchantId.Value],
                FromDate = request.From.UtcDateTime,
                ToDate = request.To.UtcDateTime.AddTicks(-1),

                IncludeOrderMenuItem = true,
                IncludePosChargeChargeInvoiceDocuments = true,
                IncludePosChargeChargeMerchantCustomChargeCustomChargeMethod = true,

                PageIndex = 0,
                PageSize = null,
            });

            var result = preResult.OrderByDescending(d => d.PosCharge!.CaptureDate)
                                    .ThenBy(d => d.ParentPosChargeInvoiceItemId ?? d.Id)
                                    .ThenBy(d => d.Id);

            var exportBuilder = dataExporter.Create(result)
                                            .AddColumn(request.Labels.Date, x => x.PosCharge!.CaptureDate)
                                            .AddColumn(request.Labels.TransactionId, x => idConverter.ToPublicId(x.PosChargeId))
                                            .AddColumn(request.Labels.Invoice, x => x.PosCharge!.Charge!.InvoiceDocuments!.FirstOrDefault(s => s.DocumentType == InvoiceDocumentType.OrderInvoice)?.DocumentId ?? "")
                                            .AddColumn(request.Labels.Id, x => idConverter.ToPublicId(x.Id))
                                            .AddColumn(request.Labels.Method, x => x.PosCharge!.Charge!.ChargeMethod == ChargeMethod.Custom ? (x.PosCharge!.Charge.MerchantCustomCharge?.CustomChargeMethod!.Name ?? "-") : "Quivi")
                                            .AddColumn(request.Labels.MenuId, x => x.ParentPosChargeInvoiceItemId.HasValue ? idConverter.ToPublicId(x.ParentPosChargeInvoiceItemId.Value) : idConverter.ToPublicId(x.Id))
                                            .AddColumn(request.Labels.Item, x => x.OrderMenuItem!.Name)
                                            .AddColumn(request.Labels.UnitPrice, x => x.OrderMenuItem!.FinalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                                            .AddColumn(request.Labels.Quantity, x => x.Quantity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                                            .AddColumn(request.Labels.Total, x => (x.Quantity * x.OrderMenuItem!.FinalPrice).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));

            using (var memoryStream = await exportBuilder.ExportAsync(type))
            {
                return new ExportSalesResponse
                {
                    Name = dataExporter.GetExportName(type, $"Sales {request.From.ToString("yyyy-MM-dd")} - {request.To.ToString("yyyy-MM-dd")}"),
                    Data = Convert.ToBase64String(memoryStream.ToByteArray()),
                };
            }
        }
    }
}