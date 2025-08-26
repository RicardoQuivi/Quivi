using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quivi.Application.Attributes;
using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Backoffice.Api.Requests.MerchantDocuments;
using Quivi.Backoffice.Api.Responses.MerchantDocuments;
using Quivi.Domain.Entities.Pos;
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
    public class MerchantDocumentsController : ControllerBase
    {
        private readonly IQueryProcessor queryProcessor;
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public MerchantDocumentsController(IQueryProcessor queryProcessor,
                                            IMapper mapper,
                                            IIdConverter idConverter)
        {
            this.queryProcessor = queryProcessor;
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        [HttpGet]
        public async Task<GetMerchantDocumentsResponse> Get([FromQuery] GetMerchantDocumentsRequest request)
        {
            request ??= new();
            var query = await queryProcessor.Execute(new GetMerchantInvoiceDocumentsAsyncQuery
            {
                MerchantIds = [User.SubMerchantId(idConverter)!.Value],
                PosChargeIds = request.TransactionIds?.Select(idConverter.FromPublicId),
                Types = request.MonthlyInvoiceOnly ? [InvoiceDocumentType.MerchantMonthlyInvoice] : null,
                Formats = [DocumentFormat.Pdf],
                HasDownloadPath = true,

                PageIndex = request.Page,
                PageSize = request.PageSize,
            });

            return new GetMerchantDocumentsResponse
            {
                Data = mapper.Map<Dtos.MerchantDocument>(query),
                Page = query.CurrentPage,
                TotalPages = query.NumberOfPages,
                TotalItems = query.TotalItems,
            };
        }
    }
}