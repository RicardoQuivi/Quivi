using Quivi.Application.Queries.MerchantInvoiceDocuments;
using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class MerchantInvoiceDocumentMapperHandler : IMapperHandler<MerchantInvoiceDocument, Dtos.MerchantDocument>
    {
        private readonly IIdConverter idConverter;

        public MerchantInvoiceDocumentMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public MerchantDocument Map(MerchantInvoiceDocument model)
        {
            if (model.Path == null)
                throw new Exception($"No ${nameof(model.Path)} was set to {nameof(MerchantInvoiceDocument)}. Please make sure to set {nameof(GetMerchantInvoiceDocumentsAsyncQuery.HasDownloadPath)} when using {nameof(GetMerchantInvoiceDocumentsAsyncQuery)}.");

            return new MerchantDocument
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.DocumentId ?? "",
                DownloadUrl = model.Path,
                CreatedDate = new DateTimeOffset(model.CreatedDate, TimeSpan.Zero),
            };
        }
    }
}