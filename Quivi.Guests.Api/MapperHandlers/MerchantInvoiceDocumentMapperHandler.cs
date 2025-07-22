using Quivi.Domain.Entities.Pos;
using Quivi.Guests.Api.Dtos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Guests.Api.MapperHandlers
{
    public class MerchantInvoiceDocumentMapperHandler : IMapperHandler<MerchantInvoiceDocument, Dtos.TransactionInvoice>
    {
        private readonly IIdConverter idConverter;

        public MerchantInvoiceDocumentMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public TransactionInvoice Map(MerchantInvoiceDocument model)
        {
            if (model.ChargeId.HasValue == false)
                throw new Exception($"Only {nameof(MerchantInvoiceDocument)} with {nameof(MerchantInvoiceDocument.ChargeId)} can be mapped to {nameof(Dtos.TransactionInvoice)}");

            if (string.IsNullOrWhiteSpace(model.Path))
                throw new Exception($"Only {nameof(MerchantInvoiceDocument)} with {nameof(MerchantInvoiceDocument.Path)} can be mapped to {nameof(Dtos.TransactionInvoice)}");

            if (string.IsNullOrWhiteSpace(model.DocumentId))
                throw new Exception($"Only {nameof(MerchantInvoiceDocument)} with {nameof(MerchantInvoiceDocument.DocumentId)} can be mapped to {nameof(Dtos.TransactionInvoice)}");

            return new TransactionInvoice
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.DocumentId,
                Type = model.DocumentType,
                TransactionId = idConverter.ToPublicId(model.ChargeId.Value),
                DownloadUrl = model.Path,
            };
        }
    }
}
