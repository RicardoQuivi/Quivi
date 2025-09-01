using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Pos.Api.MapperHandlers
{
    public class MerchantInvoiceDocumentMapperHandler : IMapperHandler<MerchantInvoiceDocument, Dtos.TransactionDocument>
    {
        private readonly IMapper mapper;
        private readonly IIdConverter idConverter;

        public MerchantInvoiceDocumentMapperHandler(IMapper mapper, IIdConverter idConverter)
        {
            this.mapper = mapper;
            this.idConverter = idConverter;
        }

        public Dtos.TransactionDocument Map(MerchantInvoiceDocument model)
        {
            return new Dtos.TransactionDocument
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.DocumentId ?? "",
                Type = mapper.Map<Dtos.TransactionDocumentType>(model.DocumentType),
                Url = model.Path ?? throw new Exception(),
            };
        }
    }
}
