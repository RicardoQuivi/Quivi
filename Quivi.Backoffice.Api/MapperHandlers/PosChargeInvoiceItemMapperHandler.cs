using Quivi.Backoffice.Api.Dtos;
using Quivi.Domain.Entities.Pos;
using Quivi.Infrastructure.Abstractions.Converters;
using Quivi.Infrastructure.Abstractions.Mapping;

namespace Quivi.Backoffice.Api.MapperHandlers
{
    public class PosChargeInvoiceItemMapperHandler : IMapperHandler<PosChargeInvoiceItem, Dtos.TransactionItem>
    {
        private readonly IIdConverter idConverter;

        public PosChargeInvoiceItemMapperHandler(IIdConverter idConverter)
        {
            this.idConverter = idConverter;
        }

        public TransactionItem Map(PosChargeInvoiceItem model)
        {
            if (model.OrderMenuItem == null)
                throw new Exception($"Entity {nameof(OrderMenuItem)} was not included into {nameof(PosChargeInvoiceItem)}.");

            return new TransactionItem
            {
                Id = idConverter.ToPublicId(model.Id),
                Name = model.OrderMenuItem.Name,
                Quantity = model.Quantity,
                FinalPrice = model.OrderMenuItem.FinalPrice,
                OriginalPrice = model.OrderMenuItem.OriginalPrice,
            };
        }
    }
}